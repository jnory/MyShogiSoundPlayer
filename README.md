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

# 音が鳴らないときは

以下のいずれかのコマンドで試験実行して下さい。

 mono SoundPlayer.exe --debug [MyShogiのサウンドディレクトリへのパス]

または、

 mono SoundPlayer.exe --debug-aggressive [MyShogiのサウンドディレクトリへのパス]

音が鳴ればSoundPlayerは(おそらく)正常に動いています。
--debugと--debug-aggressiveの違いは試験的に鳴らす音の数の違いです。
--debug-aggressiveでは認識されているすべての音を一通り鳴らします。

音が鳴らない場合、上記コマンドの実行結果を添えてMyShogiSoundPlayerのリポジトリにIssueを立てるか、
作者のTwitter( https://twitter.com/arrow_elpis )までお問い合わせください。

# 現在の制約事項

Linuxで対局の途中から音が鳴らなくなる現象が発生しています。

以上
