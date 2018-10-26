# このプロジェクトについて

このプロジェクトはMyShogi (https://github.com/yaneurao/MyShogi) をMonoで動かす際に音を鳴らすためのプラグインです。

# 使い方

このリポジトリのリリースビルドからSoundPlayer.exeをダウンロードし、
MyShogi.exeと同じフォルダに置いて下さい。

# ビルド方法

1. external/libsoundio-sharp をcloneします。
2. cd external && nuget install NUnit && nuget install ILRepack
3. external/libsoundio-sharpから先ほどnugetでダウンロードしたNUnitを使うようにします
4. msbuild libsoundio-sharp.sln /p:Configuration=Release
5. プロジェクトのトップディレクトリに移動して msbuild MyShogiSoundPlayer.sln /p:Configuration=macOS または msbuild MyShogiSoundPlayer.sln /p:Configuration=Linux
6. mono external/ILRepack.2.0.16/tools/ILRepack.exe /out:MyShogiSoundPlayer/bin/Release/SoundPlayer.exe MyShogiSoundPlayer/bin/Release/MyShogiSoundPlayer.exe MyShogiSoundPlayer/bin/Release/libsoundio-sharp.dll

以上
