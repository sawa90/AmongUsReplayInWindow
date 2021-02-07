# AmongUsReplayInWindow
![screenshot](https://github.com/sawa90/AmongUsReplayInWindow/blob/images/scrnshot2.png)

Among Usにリプレイ機能を追加するツールです。リプレイはゲーム終了後にロビーに戻るとゲームウィンドウ内に表示され、コントロールキーで開閉できます。  
リプレイでは、全員の居場所、誰がインポスターか、いつ誰が殺したか、タスクの進行状況、ベント、サボタージュ（ドアの開閉除く）を見ることができます。  

# Requirement
[.NET 5 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/5.0)  
[Visual Studio 2015 Visual C++ 再頒布可能パッケージ](https://www.microsoft.com/ja-jp/download/details.aspx?id=48145)  
# Usage
- [latest release](https://github.com/sawa90/AmongUsReplayInWindow/releases/latest)からzipファイルをダウンロードして解凍してください。
- Among Usを起動してください。
- ダウンロードしたフォルダにある`AmongUsReplayInWindow.exe`を起動してください。
- 「Get Among Us Window」ボタンをクリックし、表記が「Running...」になるまで待機してください。
- そのまま通常通りゲームをプレイしてください。

ゲーム終了後にロビーまたはメニュー画面に戻るとゲームウィンドウ内にリプレイが表示されます。開閉にはコントロールキーを使用してください。     
下部のトラックバーをスライドさせることで時間を動かせます。また、矢印キーの上下で会議時間の前後までスキップできます。  
リプレイはexeがあるフォルダ内の「replay」フォルダに保存され、起動時のウィンドウにある「open」ボタンからファイルを開くことでもリプレイを見ることができます。  

# Note
- 「誰が殺したか」は死体を示すバツ印の縁取りの色で示されますが、単に殺害時一番近くにいたインポスターを示すだけなので間違っている可能性があります。
- 左上の時間表記の上に現在の状況（タスク中、会議中、サボタージュ）が表示され、ゲーム終了時には「ImpostorsWinByVote」のように勝者と勝因が表示されますが、切断で決着がついた際などは誤って表示されることがあります。
-マップ画像などは差し替えが可能です。map,iconフォルダ内に同名で画像を保存してください。マップ画像はマップの位置と画像の縦横比を元のファイルに合わせてください。
-うまく動かない場合はエラー報告をしてくださると大変助かります。[latest release](https://github.com/sawa90/AmongUsReplayInWindow/releases/latest)から_withConsoleのついたバージョンをダウンロードしてコンソールウィンドウの出力を貼っていただけるとありがたいです。出力にはエラーと表示されない場合でも何かありましたらお気軽にどうぞ。

# License
MIT  

# References
[amonguscapture](https://github.com/automuteus/amonguscapture) by Denver Quane  
[among-us-replay-mod](https://github.com/Smertig/among-us-replay-mod) by Smertig  

