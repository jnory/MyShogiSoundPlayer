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
    SoundInfo *info = (SoundInfo *) outStream->userdata;

    int32_t frameCount = min(info->nData - info->count, frameCountMax);
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

    const struct SoundIoChannelLayout *layout = &outStream->layout;
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

int playSound(
    int16_t *waveData, 
    uint32_t nData,
    uint32_t samplingRate, 
    uint16_t numChannels,
    uint32_t soundMiliSec
) 
{

    struct SoundIo *soundio = soundio_create();
    if(!soundio) {
        fprintf(stderr, "ERROR: failed to get soundio object\n");
        return 1;
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
        return 1;
    }

    soundio_flush_events(soundio); 
    int device_index = soundio_default_output_device_index(soundio);

    if (device_index < 0) {
        fprintf(stderr, "ERROR: device not found\n");
        return 1;
    }

    struct SoundIoDevice *device = soundio_get_output_device(soundio, device_index);
    if(!device) {
        fprintf(stderr, "ERROR: failed to get device; out of memory\n");
        return 1;
    }

    if (device-> probe_error) {
        fprintf(stderr, "Cannot probe device: %s\n", soundio_strerror(device->probe_error));
        return 1;
    }

    struct SoundIoOutStream *outStream = soundio_outstream_create(device);
    if(!device) {
        fprintf(stderr, "ERROR: failed to create outstream; out of memory\n");
        return 1;
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

    err = soundio_outstream_open(outStream);
    if (err) {
        fprintf(stderr, "ERROR: unable to open device: %s", soundio_strerror(err));
        return 1;
    }

    if (outStream->layout_error) {
        fprintf(stderr, "WARNING: unable to set channel layout: %s\n", soundio_strerror(outStream->layout_error));
    }

    err = soundio_outstream_start(outStream);
#ifdef MACOS
    soundio_wait_events(soundio);
#else
    while(true){
        soundio_flush_events(soundio);
        if(info->finished){
            break;
        }
        usleep(1000);
    }
#endif
    soundio_outstream_destroy(outStream);
    soundio_device_unref(device);
    soundio_destroy(soundio);
    
    deleteSoundInfo(info);
    return 0;
}