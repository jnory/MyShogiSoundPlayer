macos:
	cd wplay && make macos
	msbuild ./MyShogiSoundPlayer.sln /p:Configuration=macOS

linux:
	cd wplay && make linux
	msbuild ./MyShogiSoundPlayer.sln /p:Configuration=linux
