LIBSOUNDIO=../external/libsoundio

linux: src/wplay.c src/wplay.h libsoundio_linux/libsoundio.a
	gcc -shared -fPIC -o libwplay.so src/wplay.c libsoundio_linux/libsoundio.a -I$(LIBSOUNDIO) -DLINUX 

macos: src/wplay.c src/wplay.h libsoundio_macos/libsoundio.a
	gcc -framework CoreAudio -framework AudioUnit -framework CoreFoundation -dynamiclib -o libwplay.dylib src/wplay.c libsoundio_macos/libsoundio.a -I$(LIBSOUNDIO) -DMACOS 

libsoundio_macos/libsoundio.a:
	cd libsoundio_macos && cmake ../$(LIBSOUNDIO) && make

libsoundio_linux/libsoundio.a:
	cd libsoundio_linux && cmake ../$(LIBSOUNDIO) -DCMAKE_C_FLAGS=-fPIC && make

