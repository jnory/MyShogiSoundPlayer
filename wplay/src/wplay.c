#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <assert.h>
#include <soundio/soundio.h>
#ifndef MACOS
#include <unistd.h>
#endif

#include "wplay.h"

/*
libsoundioを使ってwavファイルを再生します。
https://github.com/andrewrk/libsoundio/blob/master/example/sio_sine.c
を一部そのまま使っています。

以下に元のライセンスを張ります。

The MIT License (Expat)

Copyright (c) 2015 Andrew Kelley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/

typedef struct soundInfo_ {
    int16_t *waveData;
    uint32_t nData;
    uint32_t samplingRate;
    uint16_t numChannels;
    uint32_t soundMiliSec;
    uint32_t count;
#ifdef LINUX
    bool finished;
#endif
} SoundInfo;

static SoundInfo *newSoundInfo(
    int16_t *waveData, 
    uint32_t nData,
    uint32_t samplingRate, 
    uint16_t numChannels,
    uint32_t soundMiliSec
){
    SoundInfo *info = (SoundInfo *)malloc(sizeof(SoundInfo));
    if (info == NULL) {
        return NULL;
    }

    info->waveData = waveData;
    info->nData = nData;
    info->samplingRate = samplingRate;
    info->numChannels = numChannels;
    info->soundMiliSec = soundMiliSec;
    info->count = 0;
#ifdef LINUX
    info->finished = false;
#endif

    return info;
}

static void deleteSoundInfo(SoundInfo *info) {
    free(info);
}

static float getSample(
    SoundInfo *info, uint16_t outNChannels, uint16_t channel) {
    uint16_t inNChannels = info->numChannels;
    assert(inNChannels == 1 || inNChannels == 2);
    assert(outNChannels == 1 || outNChannels == 2);
    assert(channel == 0 || channel == 1);

    if (info->count >= info->nData) {
        return 0.0;
    } else {
        float sample = ((float)info->waveData[info->count]) / INT16_MAX;

        if(inNChannels == 1 && channel + 1 == outNChannels) {
            info->count++;
        } else if(inNChannels == 2 && outNChannels == 1){
            info->count += inNChannels;
        } else if(inNChannels == 2) {
            info->count++;
        }

        return sample;
    }
}

int32_t min(int32_t a, int32_t b) {
    if(a < b){
        return a;
    } else {
        return b;
    }
}

int32_t max(int32_t a, int32_t b) {
    if(a > b){
        return a;
    } else {
        return b;
    }
}

static void writeCallback(struct SoundIoOutStream *outStream, int frameCountMin, int frameCountMax) {
    const struct SoundIoChannelLayout *layout = &outStream->layout;

    SoundInfo *info = (SoundInfo *) outStream->userdata;
    int32_t frameCount = info->nData - info->count;
    frameCount -= frameCount % layout->channel_count;
    frameCount /= layout->channel_count;
    frameCount = min(frameCount, frameCountMax);
    if(frameCount == 0) {
#ifdef MACOS
        soundio_wakeup(outStream->device->soundio);
#endif
        return;
    }

    frameCount = max(frameCountMin, frameCount);

    struct SoundIoChannelArea *areas;
    int err = soundio_outstream_begin_write(outStream, &areas, &frameCount);
    if(err) {
        fprintf(stderr, "%s\n", soundio_strerror(err));
        return;
    }

    for(uint32_t frame = 0; frame < frameCount; ++frame) {
        for (uint32_t channel = 0; channel < layout->channel_count; ++channel) {
            float sample = getSample(info, layout->channel_count, channel);
            *((float *)areas[channel].ptr) = sample;
            areas[channel].ptr += areas[channel].step;
        }
    }

    err = soundio_outstream_end_write(outStream);
    if(err){
        if(err == SoundIoErrorUnderflow) {
            return;
        }

        fprintf(stderr, "%s", soundio_strerror(err));
        return;
    }
}

#ifdef LINUX
static void underflowCallback(struct SoundIoOutStream *outStream) {
    SoundInfo *info = (SoundInfo *) outStream->userdata;
    info->finished = true;
}
#endif

struct SoundIo *newSoundIo() {
    struct SoundIo *soundio = soundio_create();
    if(!soundio) {
        fprintf(stderr, "ERROR: failed to get soundio object\n");
        return NULL;
    }

    enum SoundIoBackend backend;
#ifdef LINUX
    backend = SoundIoBackendPulseAudio;
#elif defined MACOS
    backend = SoundIoBackendCoreAudio;
#else
    backend = SoundIoBackendDummy;
    fprintf(stderr, "WARNING: Connecting to Dummy backend\n");
#endif

    int err = soundio_connect_backend(soundio, backend);
    if (err) {
        fprintf(stderr, "ERROR: Unable to connect to backend: %s\n", soundio_strerror(err));
        soundio_destroy(soundio);
        return NULL;
    }

    return soundio;
}

struct SoundIoDevice *getDevice(struct SoundIo *soundio) {
    soundio_flush_events(soundio); 

    int device_index = soundio_default_output_device_index(soundio);
    if (device_index < 0) {
        fprintf(stderr, "ERROR: device not found\n");
        return NULL;
    }

    struct SoundIoDevice *device = soundio_get_output_device(soundio, device_index);
    if(!device) {
        fprintf(stderr, "ERROR: failed to get device; out of memory\n");
        return NULL;
    }

    if (device-> probe_error) {
        fprintf(stderr, "Cannot probe device: %s\n", soundio_strerror(device->probe_error));
        soundio_device_unref(device);
        return NULL;
    }

    return device;
}

struct SoundIoOutStream *newOutStream(
    struct SoundIoDevice *device,
    int16_t *waveData, 
    uint32_t nData,
    uint32_t samplingRate, 
    uint16_t numChannels,
    uint32_t soundMiliSec
) {
    struct SoundIoOutStream *outStream = soundio_outstream_create(device);
    if(!device) {
        fprintf(stderr, "ERROR: failed to create outstream; out of memory\n");
        return NULL;
    }

    SoundInfo *info = newSoundInfo(waveData, nData, samplingRate, numChannels, soundMiliSec);

    outStream->userdata = info;
    outStream->write_callback = writeCallback;
#ifdef LINUX
    outStream->underflow_callback = underflowCallback;
#endif
    const struct SoundIoChannelLayout *layout = soundio_channel_layout_get_default(numChannels);
    outStream->layout = *layout;
    outStream->sample_rate = samplingRate;
    outStream->software_latency = 0.0f;

    return outStream;
}

bool kickStream(struct SoundIoOutStream *outStream) {
    int err = soundio_outstream_open(outStream);
    if (err) {
        fprintf(stderr, "ERROR: unable to open device: %s", soundio_strerror(err));
        return false;
    }

    if (outStream->layout_error) {
        fprintf(stderr, "WARNING: unable to set channel layout: %s\n", soundio_strerror(outStream->layout_error));
    }

    err = soundio_outstream_start(outStream);
    if (err) {
        fprintf(stderr, "ERROR: failed to start stream: %s", soundio_strerror(err));
        return false;
    }

    return true;
}

int playSound(
    int16_t *waveData, 
    uint32_t nData,
    uint32_t samplingRate, 
    uint16_t numChannels,
    uint32_t soundMiliSec
) 
{
    struct SoundIo *soundio = newSoundIo();
    if(soundio == NULL) {
        return 1;
    }

    struct SoundIoDevice *device = getDevice(soundio);
    if(device == NULL) {
        soundio_destroy(soundio);
        return 1;
    }

    struct SoundIoOutStream *outStream = newOutStream(
        device, waveData, nData, samplingRate, numChannels, soundMiliSec);
    if(outStream == NULL) {
        soundio_device_unref(device);
        soundio_destroy(soundio);
        return 1;
    }

    SoundInfo *info = (SoundInfo *)outStream->userdata;

    if(!kickStream(outStream)) {
        soundio_outstream_destroy(outStream);
        soundio_device_unref(device);
        soundio_destroy(soundio);
        deleteSoundInfo(info);
    }

#ifdef LINUX
    soundio_flush_events(soundio);
    usleep(soundMiliSec * 1000);
#else
    soundio_wait_events(soundio);
#endif

    soundio_outstream_destroy(outStream);
    soundio_device_unref(device);
    soundio_destroy(soundio);
    deleteSoundInfo(info);

    return 0;
}

bool checkCompatibility() {
    struct SoundIo *soundio = newSoundIo();
    if(soundio == NULL) {
        return 1;
    }

    struct SoundIoDevice *device = getDevice(soundio);
    if(device == NULL) {
        soundio_destroy(soundio);
        return 1;
    }

    bool ret = soundio_device_supports_format(device, SoundIoFormatFloat32NE);

    soundio_device_unref(device);
    soundio_destroy(soundio);
    return ret;
}


void printDebugInfo() {
    struct SoundIo *soundio = newSoundIo();
    if(soundio == NULL) {
        return;
    }
    soundio_flush_events(soundio); 

    int n_device = soundio_output_device_count(soundio);
    for(int i = 0; i < n_device; ++i) {
        fprintf(stderr, "=== Handling Device %d ===\n", i);

        struct SoundIoDevice *device = soundio_get_output_device(soundio, i);
        if (device == NULL || device->probe_error != 0) {
            fprintf(stderr, "Failed to get device %d\n", i);
            continue;
        }

        fprintf(stderr, "Device Name: %s\n", device->name);
        if(soundio_device_supports_format(device, SoundIoFormatFloat32NE)) {
            fprintf(stderr, "Float32NE\n");
        } else if (soundio_device_supports_format(device, SoundIoFormatFloat64NE)){
            fprintf(stderr, "Float64NE\n");
        } else if (soundio_device_supports_format(device, SoundIoFormatS32NE)){
            fprintf(stderr, "S32NE\n");
        } else if (soundio_device_supports_format(device, SoundIoFormatS16NE)){
            fprintf(stderr, "S16NE\n");
        } else {
            fprintf(stderr, "Unknown\n");
        }

        fprintf(stderr, "OtherFormat:\n");
        for(int i = 0; i < device->format_count; ++i) {
            fprintf(stderr, "%s\n", soundio_format_string(device->formats[i]));
        }

        fprintf(stderr, "Current Layout: %s\n", device->current_layout.name);
        fprintf(stderr, "Other Layout: \n");
        for(int i = 0; i < device->layout_count; ++i) {
            fprintf(stderr, "%s\n", device->layouts[i].name);
        }

        fprintf(stderr, "Mono is null: %s\n", soundio_channel_layout_get_default(1) == NULL?"true":"false");
        fprintf(stderr, "Stereo is null: %s\n", soundio_channel_layout_get_default(2) == NULL?"true":"false");
        fprintf(stderr, "Supports Sample Rate 44100: %s\n", soundio_device_supports_sample_rate(device, 44100)?"true":"false");
        fprintf(stderr, "Supports Sample Rate 22050: %s\n", soundio_device_supports_sample_rate(device, 22050)?"true":"false");

        soundio_device_unref(device);
    }

    fprintf(stderr, "Default output device: %d\n", soundio_default_output_device_index(soundio));
    soundio_destroy(soundio);
}