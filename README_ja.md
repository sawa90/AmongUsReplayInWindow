# AmongUsReplayInWindow
![license MIT](https://img.shields.io/badge/license-MIT-green)
![Downloads num](https://img.shields.io/github/downloads/sawa90/AmongUsReplayInWindow/total)  
![screenshot](https://github.com/sawa90/AmongUsReplayInWindow/blob/images/scrnshot3.png)

Among Us (Steam version)に簡易的なリプレイ機能を追加するツールです。リプレイはゲーム終了後ゲームウィンドウ内に表示され、コントロールキーで開閉することができます。
リプレイでは、全員の居場所、誰がインポスターか、いつ誰が殺したか、タスクの進行状況、投票結果、ベント、サボタージュを見ることができます。  
また、チャットのログ、投票結果などを含むテキストログの出力も可能です。

## Requirement
[.NET 5 Desktop Runtime x86](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-5.0.3-windows-x86-installer)  
[Visual Studio 2015 Visual C++ 再頒布可能パッケージ x86](https://www.microsoft.com/ja-jp/download/details.aspx?id=48145)  
## Usage
- [latest release](https://github.com/sawa90/AmongUsReplayInWindow/releases/latest)からzipファイルをダウンロードして解凍してください。
- Among Usを起動してください。
- 解凍したフォルダ内の`AmongUsReplayInWindow.exe`を起動してください。
- 「Get Among Us Window」ボタンをクリックし、表記が「Running...」になるまで待機してください。
- そのまま通常通りゲームをプレイしてください。

ゲーム終了後にロビーまたはメニュー画面に戻るとゲームウィンドウ内にリプレイが表示されます。開閉にはコントロールキーを使用してください。     
下部のトラックバーをスライドさせることで時間を動かせます。また、矢印キーの上下で会議時間の前後までスキップできます。  
リプレイはexeがあるフォルダ内の「replay」フォルダに保存され、exe起動時表示されるウィンドウの「open」ボタンからファイルを開くことでもリプレイを見ることができます。  

## Demo
[Youtube](https://youtu.be/qMS7B8juyDo)

## Note
- 「誰が殺したか」は死体を示すバツ印の縁取りの色で示されますが、単に殺害時一番近くにいたインポスターを示すだけなので間違っている可能性があります。
- 左上の時間表記の上に現在の状況（タスク中、会議中、サボタージュ）が表示され、ゲーム終了時には「ImpostorsWinByVote」のように勝者と勝因が表示されますが、切断で決着がついた際などは誤って表示されることがあります。
- マップ画像などは差し替えが可能です。map,iconフォルダ内に同名で画像を保存してください。マップ画像はマップの位置と画像の縦横比を元のファイルに合わせてください。
- このツールは[amonguscapture](https://github.com/automuteus/amonguscapture)を基にしたものであり、メモリの読み取りとキーボード・ウィンドウプロシージャフック、元ウィンドウに重ねた半透明のウィンドウを使って実現させています。ゲーム自体の改変やメモリの書き換えは行いません。
- うまく動かない場合はエラー報告をしてくださると大変助かります。コンソールウィンドウの出力を貼っていただけるとありがたいです。コンソールウィンドウはexeと同じフォルダに作成されるsetting.jsonをメモ帳などで開き、"Console":falseを"Console":trueに変更してから起動すると表示されます。出力にはエラーと表示されない場合でも何かありましたらお気軽にどうぞ｡

## License
ソースコードはMITライセンスです。  
画像ファイルはAmong Usの規約に従って取り扱ってください。  

## References
[amonguscapture](https://github.com/automuteus/amonguscapture) by Denver Quane  
[among-us-replay-mod](https://github.com/Smertig/among-us-replay-mod) by Smertig  
[AmongUsMemory](https://github.com/shlifedev/AmongUsMemory) by shlifedev  

##
This tool is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
