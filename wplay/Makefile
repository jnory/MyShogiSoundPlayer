LIBSOUNDIO=../external/libsoundio

linux: src/wplay.c src/wplay.h libsoundio_linux/libsoundio.a
	gcc -shared -fPIC -o libwplay.so src/wplay.c libsoundio_linux/libsoundio.a -I$(LIBSOUNDIO) -lpulse -DLINUX 

macos: src/wplay.c src/wplay.h libsoundio_macos/libsoundio.a
	gcc -framework CoreAudio -framework AudioUnit -framework CoreFoundation -dynamiclib -o libwplay.dylib src/wplay.c libsoundio_macos/libsoundio.a -I$(LIBSOUNDIO) -DMACOS 

libsoundio_macos/libsoundio.a:
	cd libsoundio_macos && cmake ../$(LIBSOUNDIO) -DENABLE_JACK=OFF -DENABLE_WASAPI=OFF -DENABLE_COREAUDIO=ON -DENABLE_ALSA=OFF -DENABLE_PULSEAUDIO=OFF && make

libsoundio_linux/libsoundio.a:
	cd libsoundio_linux && cmake ../$(LIBSOUNDIO) -DCMAKE_C_FLAGS=-fPIC -DENABLE_JACK=OFF -DENABLE_WASAPI=OFF -DENABLE_COREAUDIO=OFF -DENABLE_ALSA=OFF -DENABLE_PULSEAUDIO=ON && make

