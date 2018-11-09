# このプロジェクトについて

このプロジェクトはMyShogi (https://github.com/yaneurao/MyShogi) をMonoで動かす際に音を鳴らすためのプラグインです。

# 使い方

このリポジトリのリリースビルドからSoundPlayer.exeをダウンロードし、
MyShogi.exeと同じフォルダに置いて下さい。

# ビルド方法

## 必要なもの

* mono
* gccまたはclang
* make
* cmake
* (Linuxの場合) pulseaudioライブラリ (libpulse)
* このリポジトリ ( `git clone --recursive` を使ってサブモジュールごとcloneしてください )

## macOS

 > make macos

## Linux

 > make linux

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

以上
