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

# 不具合に気付いたときは

不具合にお気付きの時も、上記デバッグモードの実行結果を添えてMyShogiSoundPlayerのリポジトリにIssueを立てるか、
作者のTwitter( https://twitter.com/arrow_elpis )までお問い合わせください。

また、SoundPlayerにはロギングモードが実装されています。
環境変数 `MYSHOGI_SOUNDPLAYER_LOGPATH` にログの保存先ファイル名を指定してMyShogiを起動することで、
実行ログが該当ファイルに保存されます。
不具合が発生したときのログも合わせてお問い合わせいただきますと原因救命が早くなるかもしれません。
なお、ログの実行結果には棋譜の情報が含まれますので、公開不可な棋譜が混ざり込まないようご注意ください。

## ロギングモードの実行例

実行ログをsoundplayer.logに保存したいときは以下のようにします。

 MYSHOGI_SOUNDPLAYER_LOGPATH=soundplayer.log mono --arch=32 ./MyShogi.exe


以上
