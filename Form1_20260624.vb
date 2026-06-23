Imports System.IO.Ports
Imports System.Threading.Tasks

Public Class Form1



#Const MNI_ENABLE = 0'1：WMI有 0:WMI無し 

    Const RXRGSZ As Integer = 64
    'Const RXRGSZ As Integer = 2048

    Structure TYP_COMRING
        Dim rp As Integer
        Dim wp As Integer
        Dim dat() As Byte
    End Structure

    Structure COM_PORT
        Dim dvicename As String
        Dim comno As String
    End Structure

    Dim RxRing As TYP_COMRING '受信したデータを管理
    Dim ComInfo() As COM_PORT
    Dim selectComNo As Integer


    'RxRingの初期化を行う
    Private Sub RxRingInit()
        RxRing.wp = 0
        RxRing.rp = 0
        ReDim RxRing.dat(RXRGSZ - 1)
    End Sub

    'Comboboxを初期化する
    Private Sub ComboInit()

#If MNI_ENABLE Then
        ComPortChk2() 
#Else
        ComPortChk() 'ComboBox1の初期化（ポートの検索）
#End If
        ComboBox2.Items.Add("9600")
        ComboBox2.Items.Add("19200")
        ComboBox2.Items.Add("115200")
        'ComboBox2.Text = "19200"
        ComboBox2.Text = "57600"
    End Sub

    '存在するポートを検索する
    Private Sub ComPortChk()
        Dim comno() As String = SerialPort.GetPortNames
        Dim i As Integer

        ComboBox1.Items.Clear() 'アイテムをクリア

        If comno.Length = 0 Then
            ComboBox1.Text = "COM無し"
        Else
            For i = 0 To comno.Length - 1
                ComboBox1.Items.Add(comno(i))
            Next
            ComboBox1.Text = comno(0)
        End If

    End Sub

    '存在するポートを検索する
    Private Sub ComPortChk2()
        'Dim mngstr As New Management.ManagementObjectSearcher("Select * from Win32_SerialPort")
        'Dim mc As Management.ManagementObjectCollection
        'Dim serial As Management.ManagementBaseObject
        Dim comno() As String = SerialPort.GetPortNames
        Dim i, j As Integer
        Dim serialcnt As Integer
        Dim comcnt As Integer
        Dim strno As Byte
        Dim str As String
        Dim comstr As String

        'mc = mngstr.Get()
        'serialcnt = mc.Count
        comcnt = comno.Length
        ReDim ComInfo(serialcnt - 1)

        'For Each serial In mc
        'ComInfo(i).dvicename = serial("Name")

        'strno = InStr(ComInfo(i).dvicename, "(COM")
        'If strno <> 0 Then
        'strno += 1
        'comstr = ""
        'For j = 0 To 6
        'str = Mid(ComInfo(i).dvicename, strno + j, 1)
        'If str <> ")" Then
        'comstr &= str
        'Else
        'Exit For
        'End If
        'Next
        'ComInfo(i).comno = comstr
        'End If

        'i += 1
        'Next

        ComboBox1.Items.Clear() 'アイテムをクリア

        If serialcnt = 0 Then
            ComboBox1.Text = "COMポートがみつかりません。"
        Else
            For i = 0 To ComInfo.Length - 1
                ComboBox1.Items.Add(ComInfo(i).dvicename)
            Next
            ComboBox1.Text = ComInfo(0).dvicename
        End If

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

#If MNI_ENABLE Then
        ComPortChk2() '存在するポートを検索する
#Else
        ComPortChk() '存在するポートを検索する
#End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        ' RGB値を指定してボタンの背景色を設定（ここでは赤色）
        Dim red As Integer = 155
        Dim green As Integer = 230
        Dim blue As Integer = 253

        MsgBox("BaudRate = " & SerialPort1.BaudRate)  '20260622

        If com_flg = 0 Then
            com_flg = 1
            PortOpen() 'ポートをオープンする
        Else
            com_flg = 0 'ポートをクローズする
            'run_flg = 0
            'ExecuteCheckBox1()
            ExecuteCheckBoxChanged()
            'System.Threading.Thread.Sleep(1000) 'close時のハング対策
            PortClose()
        End If

        If com_flg = 1 Then
            Button2.Text = "COMポート接続中"
            Button2.BackColor = Color.FromArgb(red, green, blue)
            Button2.Enabled = False
        Else
            Button2.Text = "COMポート未接続"
            Button2.BackColor = Color.White
        End If






        'PortOpen() 'ポートをオープンする
    End Sub


    '    Private Sub ExecuteCheckBox1()
    '        ' Button1のクリックイベントを手動で呼び出す
    '        CheckBox1_CheckedChanged(CheckBox1, EventArgs.Empty)
    '    End Sub

    'ポートをクローズする
    Private Sub PortClose()
        'Me.Invoke(dlg, New Object() {dat}) 'Form1_Shownで表示できたので不要
        'Me.Invoke(dlg2, New Object() {dat})
        Try
            'With SerialPort1
            '    .Close() 'ポートを閉じる
            ''MsgBox(ComboBox1.Text & "をクローズできた。", MsgBoxStyle.OkOnly)
            'End With

            ＇''If SerialPort1.IsOpen Then 'ポートが開いている場合にのみ Close メソッドを呼び出します。
            '''   ＇これにより、ポートが既に閉じている場合に Close を
            '''   '呼び出してエラーが発生するのを防ぐことができます。
            '''    SerialPort1.Close()
            '''End If

            RemoveHandler SerialPort1.DataReceived, AddressOf SerialPort1_DataReceived

            ' 既存のデータ処理が完了するまで待機
            Threading.Thread.Sleep(1000)

            SerialPort1.Close()


            'Timer1.Enabled = True
        Catch ex As Exception
            MsgBox(ComboBox1.Text & "をクローズできませんでした。", MsgBoxStyle.OkOnly)
        End Try
    End Sub


    'ポートをオープンする
    Private Sub PortOpen()
        'Me.Invoke(dlg, New Object() {dat}) 'Form1_Shownで表示できたので不要
        'Me.Invoke(dlg2, New Object() {dat})
        Try
            With SerialPort1
                'ポートが既に開いているかどうかをチェックします。もし開いている場合は、Close メソッドを呼び出してから再度 Openする
                If SerialPort1.IsOpen Then
                    SerialPort1.Close()
                End If
                System.Threading.Thread.Sleep(1000)
                '.Close() 'ポートを閉じる

#If MNI_ENABLE Then
                .PortName = ComInfo(selectComNo).comno
#Else
                .PortName = ComboBox1.Text
#End If
                .BaudRate = ComboBox2.Text
                .DataBits = 8
                .Parity = Parity.None
                .StopBits = StopBits.One
                .Handshake = Handshake.None
                .RtsEnable = False
                .DtrEnable = False
                '.Open() 'ポートをオープンする
                SerialPort1.Open()
                MsgBox(ComboBox1.Text & "をオープンできました。", MsgBoxStyle.OkOnly)
            End With

            'Timer1.Enabled = True
        Catch ex As Exception
            MsgBox(ComboBox1.Text & "をオープンできませんでした。", MsgBoxStyle.OkOnly)
        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SerialWrite()
    End Sub

    Private Sub SerialWrite()

        If SerialPort1.IsOpen Then
            SerialPort1.Write(TextBox1.Text)
        End If

    End Sub

    Dim data_out_res As Integer = 0 '2024.10.26
    '0330 Dim buf_chk(32) As Byte  '2024.10.26
    '0330 Dim buf_chk(93) As Byte  '2025.03.30　31*3=93
    '''Dim buf_chk(95) As Byte  '2025.03.30　31*3=93
    Dim buf_chk(134) As Byte  '2026.06.21　


    Dim count_32 As Integer = 0 '2024.10.26

    Dim i As Integer = 0
    Dim i2 As Integer = 0
    Dim x As Single = 0 '■座標ｘ１初期値 ０
    Dim xx As Single = 0 '■座標ｙ２
    Dim x2 As Single = 0 '■座標ｘ１初期値 ０
    Dim xx2 As Single = 0 '■座標ｙ２

    Delegate Sub DisplayTextDelegate(ByVal dat As Short)
    Delegate Sub DisplayTextAllDelegate(ByRef datz() As Short, ByRef daty() As Short, ByRef datx() As Short)

    'Dim ii As Single = 0  '■繰り返し計算用
    Dim Zy As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Zyy As Single '■座標ｘ２
    Dim Yy As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Yyy As Single '■座標ｘ２
    Dim Xy As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Xyy As Single '■座標ｘ２

    Dim Zy2 As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Zyy2 As Single '■座標ｘ２
    Dim Yy2 As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Yyy2 As Single '■座標ｘ２
    Dim Xy2 As Single = 100 '■座標ｙ１初期値 １００（オフセット）
    Dim Xyy2 As Single '■座標ｘ２

    Private Sub DisplayText_init(ByVal dat As Short)

        'テキストBOXに文字列を追加  
        'Me.TextBox1.Text &= dat & " "

        '描画スタート
        'Static Dim x As Single = 0 '■座標ｘ１初期値 ０
        Static Dim y As Single = 100 '■座標ｙ１初期値 １００（オフセット）
        Static Dim i As Single '■繰り返し計算用
        Static Dim yy As Single '■座標ｘ２
        Static Dim g As Graphics = PictureBox1.CreateGraphics '■PictureBox1に書く
        Dim blackPen As New Pen(Color.Black, 1)
        Dim RedPen As New Pen(Color.Red, 2)

        'blackPen.DashStyle = DashStyle.Dot

        ''g.DrawLine(Pens.Black, 0, 150, 400, 150)
        ''For i = 1 To 15
        ''    g.DrawLine(blackPen, 0, 150 + i * 10, 400, 150 + i * 10)
        ''Next
        ''For i = 1 To 15
        ''    g.DrawLine(blackPen, 0, 150 - i * 10, 400, 150 - i * 10)
        ''Next
        ''For i = 1 To 39
        ''For i = 0 To 39
        ''    g.DrawLine(blackPen, i * 10, 0, i * 10, 300)
        ''Next

        blackPen.Width = 1.5

        g.DrawLine(blackPen, 0, 150, 400, 150)

        'blackPen.DashStyle = Drawing2D.DashStyle.Dash
        blackPen.DashStyle = Drawing2D.DashStyle.Dot

        For i2 = 2 To 8 Step 2
            g.DrawLine(blackPen, i2 * 50, 0, i2 * 50, 400)
        Next

        For i2 = 1 To 2
            g.DrawLine(blackPen, 0, i2 * 50, 400, i2 * 50)
        Next


        'Dim num As Integer = Integer.Parse(strDisp)

        'If (i = 0) Then
        '    y = -(dat - 128) + 128 '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    'g.DrawLine(Pens.Red, x, y, x, y) '■ライン描画 始点x,y ～ 終点xx,yy
        '    g.DrawLine(RedPen, x, y, x, y)
        '    i = i + 1
        'Else

        '    yy = -(dat - 128) + 128 '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    'xx = xx + 10 '■座標xxの計算 （*10で拡大）
        '    xx = xx + 1 '■座標xxの計算 （*10で拡大）
        '    'g.DrawLine(Pens.Red, x, y, xx, yy) '■ライン描画 始点x,y ～ 終点xx,yy
        '    g.DrawLine(RedPen, x, y, xx, yy)
        '    x = xx '■終点xxを次の始点xとする
        '    y = yy '■終点yyを次の始点yとする
        'End If


    End Sub


    Private Sub DisplayText_init2(ByVal dat As Short)

        'テキストBOXに文字列を追加  
        'Me.TextBox1.Text &= dat & " "

        '描画スタート
        'Static Dim x As Single = 0 '■座標ｘ１初期値 ０
        Static Dim y As Single = 100 '■座標ｙ１初期値 １００（オフセット）
        Static Dim i2 As Single '■繰り返し計算用
        Static Dim yy As Single '■座標ｘ２
        'Static Dim xx As Single '■座標ｙ２
        'Static Dim xx As Single '■座標ｙ２
        Static Dim g As Graphics = PictureBox2.CreateGraphics '■PictureBox1に書く
        'Dim blackPen As New Pen(Color.Black, 1)
        'Dim blackPen As New Pen(Color.Black, 0.1)
        Dim blackPen As New Pen(Color.Black, 2)
        Dim RedPen As New Pen(Color.Red, 2)

        'blackPen.DashStyle = DashStyle.Dot

        ''blackPen.Width = 0.1
        ''g.DrawLine(Pens.Black, 0, 150, 400, 150)
        ''For i2 = 1 To 15
        ''    g.DrawLine(blackPen, 0, 150 + i2 * 10, 400, 150 + i2 * 10)
        ''Next
        ''For i2 = 1 To 15
        ''    g.DrawLine(blackPen, 0, 150 - i2 * 10, 400, 150 - i2 * 10)
        ''Next
        ''For i = 1 To 39
        ''    For i2 = 0 To 39
        ''        g.DrawLine(blackPen, i2 * 10, 0, i2 * 10, 300)
        ''    Next

        blackPen.Width = 1.5

        g.DrawLine(blackPen, 0, 150, 400, 150)

        'blackPen.DashStyle = Drawing2D.DashStyle.Dash
        blackPen.DashStyle = Drawing2D.DashStyle.Dot

        For i2 = 2 To 8 Step 2
            g.DrawLine(blackPen, i2 * 50, 0, i2 * 50, 400)
        Next

        For i2 = 1 To 2
            g.DrawLine(blackPen, 0, i2 * 50, 400, i2 * 50)
        Next



        'Dim num As Integer = Integer.Parse(strDisp)

        'If (i = 0) Then
        '    y = -(dat - 128) + 128 '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    'g.DrawLine(Pens.Red, x, y, x, y) '■ライン描画 始点x,y ～ 終点xx,yy
        '    g.DrawLine(RedPen, x, y, x, y)
        '    i = i + 1
        'Else

        '    yy = -(dat - 128) + 128 '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    'xx = xx + 10 '■座標xxの計算 （*10で拡大）
        '    xx = xx + 1 '■座標xxの計算 （*10で拡大）
        '    'g.DrawLine(Pens.Red, x, y, xx, yy) '■ライン描画 始点x,y ～ 終点xx,yy
        '    g.DrawLine(RedPen, x, y, xx, yy)
        '    x = xx '■終点xxを次の始点xとする
        '    y = yy '■終点yyを次の始点yとする
        'End If



    End Sub


    Dim dlg As New DisplayTextDelegate(AddressOf DisplayText_init)
    Dim dlg2 As New DisplayTextDelegate(AddressOf DisplayText_init2)

    Dim dat As Short = 20

    Dim Dispx1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispx2() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispx3() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispy1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispy2() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispy3() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispz1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispz2() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispz3() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}


    '0330 Dim Dispz1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    '0330 Dim Dispy1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    '0330 Dim Dispx1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}

    'Dim Dispz1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    'Dim Dispy1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    'Dim Dispx1() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispi1 As Integer = 0
    Dim k1 As Integer = 0  ' local variable for statement 

    '0330 Dim Dispz0A(40) As Short  '2024.11.19
    '0330 Dim Dispy0A(40) As Short  '2024.11.19
    '0330 Dim Dispx0A(40) As Short  '2024.11.19
    Dim Dispx0A(2, 40) As Short  '2025.03.30


    Private Sub DisplayTextAll(ByRef datz() As Short, ByRef daty() As Short, ByRef datx() As Short)

        Static Dim g As Graphics = PictureBox1.CreateGraphics '■PictureBox1に書く

        Dim h As Graphics = PictureBox1.CreateGraphics()


        Dim blackPen As New Pen(Color.Black, 1)
        Dim RedPen As New Pen(Color.Red, 2)
        Dim GreenPen As New Pen(Color.Green, 2)
        Dim BluePen As New Pen(Color.Blue, 2)

        Dim i As Integer = 0  ' local variable for statement 

        g.DrawLine(Pens.Black, 0, 150, 400, 150)

        RedPen.Width = 1
        GreenPen.Width = 1
        BluePen.Width = 1

        If run_flg = 1 Then 'run_flg=1の時だけ、更新
            For i = 39 To 0 Step -1
                Dispx1(i + 1) = Dispx1(i)
                Dispx2(i + 1) = Dispx2(i)
                Dispx3(i + 1) = Dispx3(i)
            Next

            Dispx1(0) = 150 - datx(0)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
            Dispx2(0) = 150 - datx(1)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
            Dispx3(0) = 150 - datx(2)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        End If

        If expand_flg = 1 Then  'expand_flg=1の時だけ、2倍に拡大
            For i = 40 To 0 Step -1
                Dispx0A(0, i) = 150 - (150 - Dispx1(i)) * 2
                Dispx0A(1, i) = 150 - (150 - Dispx2(i)) * 2
                Dispx0A(2, i) = 150 - (150 - Dispx3(i)) * 2
            Next
        Else
            For i = 40 To 0 Step -1
                Dispx0A(0, i) = Dispx1(i)
                Dispx0A(1, i) = Dispx2(i)
                Dispx0A(2, i) = Dispx3(i)
            Next
        End If

        h.Clear(Color.White)

        Me.Invoke(dlg, New Object() {dat})

        For i = 0 To 39

            '2025.04.03
            If CheckBox3.Checked Then
                g.DrawLine(BluePen, (i * 10), Dispx0A(0, i), ((i + 1) * 10), Dispx0A(0, i + 1))   ' dispay diff
            End If

            If CheckBox4.Checked Then
                g.DrawLine(RedPen, (i * 10), Dispx0A(1, i), ((i + 1) * 10), Dispx0A(1, i + 1))   ' dispay diff
            End If

            If CheckBox5.Checked Then
                g.DrawLine(GreenPen, (i * 10), Dispx0A(2, i), ((i + 1) * 10), Dispx0A(2, i + 1))   ' dispay diff
            End If

        Next



    End Sub

    Private Sub DisplayTextAllstill(ByRef datz() As Short, ByRef daty() As Short, ByRef datx() As Short)

        Static Dim g As Graphics = PictureBox1.CreateGraphics '■PictureBox1に書く

        Dim h As Graphics = PictureBox1.CreateGraphics()


        Dim blackPen As New Pen(Color.Black, 1)
        Dim RedPen As New Pen(Color.Red, 2)
        Dim GreenPen As New Pen(Color.Green, 2)
        Dim BluePen As New Pen(Color.Blue, 2)

        Dim i As Integer = 0  ' local variable for statement 

        g.DrawLine(Pens.Black, 0, 150, 400, 150)

        RedPen.Width = 1
        GreenPen.Width = 1
        BluePen.Width = 1

        'If run_flg = 1 Then 'run_flg=1の時だけ、更新
        '    For i = 39 To 0 Step -1
        '        Dispx1(i + 1) = Dispx1(i)
        '        Dispx2(i + 1) = Dispx2(i)
        '        Dispx3(i + 1) = Dispx3(i)
        '    Next

        '    Dispx1(0) = 150 - datx(0)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispx2(0) = 150 - datx(1)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispx3(0) = 150 - datx(2)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        'End If

        If expand_flg = 1 Then  'expand_flg=1の時だけ、2倍に拡大
            For i = 40 To 0 Step -1
                Dispx0A(0, i) = 150 - (150 - Dispx1(i)) * 2
                Dispx0A(1, i) = 150 - (150 - Dispx2(i)) * 2
                Dispx0A(2, i) = 150 - (150 - Dispx3(i)) * 2
            Next
        Else
            For i = 40 To 0 Step -1
                Dispx0A(0, i) = Dispx1(i)
                Dispx0A(1, i) = Dispx2(i)
                Dispx0A(2, i) = Dispx3(i)
            Next
        End If

        h.Clear(Color.White)

        Me.Invoke(dlg, New Object() {dat})

        For i = 0 To 39

            '2025.04.03
            If CheckBox3.Checked Then
                g.DrawLine(BluePen, (i * 10), Dispx0A(0, i), ((i + 1) * 10), Dispx0A(0, i + 1))   ' dispay diff
            End If

            If CheckBox4.Checked Then
                g.DrawLine(RedPen, (i * 10), Dispx0A(1, i), ((i + 1) * 10), Dispx0A(1, i + 1))   ' dispay diff
            End If

            If CheckBox5.Checked Then
                g.DrawLine(GreenPen, (i * 10), Dispx0A(2, i), ((i + 1) * 10), Dispx0A(2, i + 1))   ' dispay diff
            End If

        Next



    End Sub



    'Dim Dispz() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    'Dim Dispy() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    'Dim Dispx() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}

    '0330 Dim Dispz() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    '0330 Dim Dispy() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    '0330 Dim Dispx() As Short = {150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150}
    Dim Dispi As Integer = 0

    Dim k2 As Integer = 0  ' local variable for statement 

    '0330 Dim Dispz1A(40) As Short  '2024.11.19
    '0330 Dim Dispy1A(40) As Short  '2024.11.19
    '0330 Dim Dispx1A(40) As Short  '2024.11.19
    Dim Dispz1A(2, 40) As Short  '2025.03.30
    Dim Dispy1A(2, 40) As Short  '2025.03.30
    'Dim Dispx1A(40) As Short  '2025.03.30


    Private Sub DisplayTextAll2(ByRef datz() As Short, ByRef daty() As Short, ByRef datx() As Short)

        Static Dim g As Graphics = PictureBox2.CreateGraphics '■PictureBox1に書く

        Dim h As Graphics = PictureBox2.CreateGraphics()


        Dim blackPen As New Pen(Color.Black, 1)

        'Dim RedPen As New Pen(Color.Red, 2)
        'Dim GreenPen As New Pen(Color.Green, 2)
        'Dim BluePen As New Pen(Color.Blue, 2)
        '色を薄くする
        Dim RedPen As New Pen(Color.FromArgb(128, 255, 0, 0), 2)
        Dim GreenPen As New Pen(Color.FromArgb(128, 0, 255, 0), 2)
        Dim BluePen As New Pen(Color.FromArgb(128, 0, 0, 255), 2)

        Dim MizuiroPen As New Pen(Color.FromArgb(173, 216, 230)) ' 水色
        Dim UsuorangePen As New Pen(Color.FromArgb(255, 200, 150)) ' 薄オレンジ色
        Dim KimidoriPen As New Pen(Color.FromArgb(50, 205, 50)) ' 黄緑色

        Dim LightMizuiroPen As New Pen(Color.FromArgb(173, 216, 230, 50)) ' 薄い水色
        Dim LightUsuorangePen As New Pen(Color.FromArgb(255, 200, 150, 50)) ' 薄い薄オレンジ色
        Dim LightKimidoriPen As New Pen(Color.FromArgb(50, 205, 50, 50)) ' 薄い黄緑色

        Dim i2 As Integer = 0  ' local variable for statement 


        g.DrawLine(Pens.Black, 0, 150, 400, 150)

        RedPen.Width = 1
        GreenPen.Width = 1
        BluePen.Width = 1


        If run_flg = 1 Then 'run_flg=1の時だけ、更新
            'For i2 = 38 To 0 Step -1
            For i2 = 39 To 0 Step -1
                Dispz1(i2 + 1) = Dispz1(i2)
                Dispy1(i2 + 1) = Dispy1(i2)
                Dispz2(i2 + 1) = Dispz2(i2)
                Dispy2(i2 + 1) = Dispy2(i2)
                Dispz3(i2 + 1) = Dispz3(i2)
                Dispy3(i2 + 1) = Dispy3(i2)
                'Dispx(i2 + 1) = Dispx(i2)
            Next
            Dispz1(0) = 150 - datz(0)  'PH0のz
            Dispy1(0) = 150 - daty(0)  'PH0のy
            Dispz2(0) = 150 - datz(1)  'PH1のz
            Dispy2(0) = 150 - daty(1)  'PH1のy
            Dispz3(0) = 150 - datz(2)  'PH2のz
            Dispy3(0) = 150 - daty(2)  'PH2のy
        End If


        If expand_flg = 1 Then  'expand_flg=1の時だけ、2倍に拡大
            For i = 40 To 0 Step -1
                'For i = 38 To 0 Step -1
                Dispz1A(0, i) = 150 - (150 - Dispz1(i)) * 2
                Dispy1A(0, i) = 150 - (150 - Dispy1(i)) * 2
                Dispz1A(1, i) = 150 - (150 - Dispz2(i)) * 2
                Dispy1A(1, i) = 150 - (150 - Dispy2(i)) * 2
                Dispz1A(2, i) = 150 - (150 - Dispz3(i)) * 2
                Dispy1A(2, i) = 150 - (150 - Dispy3(i)) * 2
            Next
        Else
            For i = 40 To 0 Step -1
                'For i = 38 To 0 Step -1
                Dispz1A(0, i) = Dispz1(i) 'PH0のz
                Dispy1A(0, i) = Dispy1(i) 'PH0のy
                Dispz1A(1, i) = Dispz2(i) 'PH1のz
                Dispy1A(1, i) = Dispy2(i) 'PH1のy
                Dispz1A(2, i) = Dispz3(i) 'PH2のz
                Dispy1A(2, i) = Dispy3(i) 'PH2のy
            Next
        End If


        h.Clear(Color.White)

        Me.Invoke(dlg2, New Object() {dat})

        For i2 = 0 To 39

            '2025.04.03
            If CheckBox8.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(0, i2), ((i2 + 1) * 10), Dispz1A(0, i2 + 1))   'PH0のz ' PH0のuseful
                g.DrawLine(MizuiroPen, (i2 * 10), Dispz1A(0, i2), ((i2 + 1) * 10), Dispz1A(0, i2 + 1))   'PH0のz ' PH0のuseful
            End If

            If CheckBox11.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(0, i2), ((i2 + 1) * 10), Dispy1A(0, i2 + 1)) 'PH0のy ' PH0のaverage
                g.DrawLine(LightMizuiroPen, (i2 * 10), Dispy1A(0, i2), ((i2 + 1) * 10), Dispy1A(0, i2 + 1)) 'PH0のy ' PH0のaverage
            End If

            If CheckBox7.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(1, i2), ((i2 + 1) * 10), Dispz1A(1, i2 + 1))   'PH1のz ' PH1のuseful
                g.DrawLine(UsuorangePen, (i2 * 10), Dispz1A(1, i2), ((i2 + 1) * 10), Dispz1A(1, i2 + 1))   'PH1のz ' PH1のuseful
            End If

            If CheckBox10.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(1, i2), ((i2 + 1) * 10), Dispy1A(1, i2 + 1)) 'PH1のy ' PH1のaverage
                g.DrawLine(LightUsuorangePen, (i2 * 10), Dispy1A(1, i2), ((i2 + 1) * 10), Dispy1A(1, i2 + 1)) 'PH1のy ' PH1のaverage
            End If

            If CheckBox6.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(2, i2), ((i2 + 1) * 10), Dispz1A(2, i2 + 1))   'PH2のz ' PH2のuseful
                g.DrawLine(KimidoriPen, (i2 * 10), Dispz1A(2, i2), ((i2 + 1) * 10), Dispz1A(2, i2 + 1))   'PH2のz ' PH2のuseful
            End If

            If CheckBox9.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(2, i2), ((i2 + 1) * 10), Dispy1A(2, i2 + 1)) 'PH2のy ' PH2のaverage
                g.DrawLine(LightKimidoriPen, (i2 * 10), Dispy1A(2, i2), ((i2 + 1) * 10), Dispy1A(2, i2 + 1)) 'PH2のy ' PH2のaverage
            End If

        Next


    End Sub

    Private Sub DisplayTextAll2still(ByRef datz() As Short, ByRef daty() As Short, ByRef datx() As Short)


        Static Dim g As Graphics = PictureBox2.CreateGraphics '■PictureBox1に書く

        Dim h As Graphics = PictureBox2.CreateGraphics()


        Dim blackPen As New Pen(Color.Black, 1)
        Dim RedPen As New Pen(Color.Red, 2)
        Dim GreenPen As New Pen(Color.Green, 2)
        Dim BluePen As New Pen(Color.Blue, 2)

        Dim MizuiroPen As New Pen(Color.FromArgb(173, 216, 230)) ' 水色
        Dim UsuorangePen As New Pen(Color.FromArgb(255, 200, 150)) ' 薄オレンジ色
        Dim KimidoriPen As New Pen(Color.FromArgb(50, 205, 50)) ' 黄緑色

        Dim LightMizuiroPen As New Pen(Color.FromArgb(173, 216, 230, 50)) ' 薄い水色
        Dim LightUsuorangePen As New Pen(Color.FromArgb(255, 200, 150, 50)) ' 薄い薄オレンジ色
        Dim LightKimidoriPen As New Pen(Color.FromArgb(50, 205, 50, 50)) ' 薄い黄緑色


        Dim i2 As Integer = 0  ' local variable for statement 


        g.DrawLine(Pens.Black, 0, 150, 400, 150)

        RedPen.Width = 1
        GreenPen.Width = 1
        BluePen.Width = 1


        'If run_flg = 1 Then 'run_flg=1の時だけ、更新
        '    'For i2 = 38 To 0 Step -1
        '    For i2 = 39 To 0 Step -1
        '        Dispz1(i2 + 1) = Dispz1(i2)
        '        Dispy1(i2 + 1) = Dispy1(i2)
        '        Dispz2(i2 + 1) = Dispz2(i2)
        '        Dispy2(i2 + 1) = Dispy2(i2)
        '        Dispz3(i2 + 1) = Dispz3(i2)
        '        Dispy3(i2 + 1) = Dispy3(i2)
        '        'Dispx(i2 + 1) = Dispx(i2)
        '    Next
        '    Dispz1(0) = 150 - datz(0)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispy1(0) = 150 - daty(0)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispz2(0) = 150 - datz(1)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispy2(0) = 150 - daty(1)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispz3(0) = 150 - datz(2)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        '    Dispy3(0) = 150 - daty(2)  '■座標yyの計算（"-"で上下反転，*10で拡大，+100でオフセット）
        'End If


        If expand_flg = 1 Then  'expand_flg=1の時だけ、2倍に拡大
            For i = 40 To 0 Step -1
                'For i = 38 To 0 Step -1
                Dispz1A(0, i) = 150 - (150 - Dispz1(i)) * 2
                Dispy1A(0, i) = 150 - (150 - Dispy1(i)) * 2
                Dispz1A(1, i) = 150 - (150 - Dispz2(i)) * 2
                Dispy1A(1, i) = 150 - (150 - Dispy2(i)) * 2
                Dispz1A(2, i) = 150 - (150 - Dispz3(i)) * 2
                Dispy1A(2, i) = 150 - (150 - Dispy3(i)) * 2
            Next
        Else
            For i = 40 To 0 Step -1
                'For i = 38 To 0 Step -1
                Dispz1A(0, i) = Dispz1(i)
                Dispy1A(0, i) = Dispy1(i)
                Dispz1A(1, i) = Dispz2(i)
                Dispy1A(1, i) = Dispy2(i)
                Dispz1A(2, i) = Dispz3(i)
                Dispy1A(2, i) = Dispy3(i)
            Next
        End If


        h.Clear(Color.White)

        Me.Invoke(dlg2, New Object() {dat})

        For i2 = 0 To 39

            '2025.04.03
            If CheckBox8.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(0, i2), ((i2 + 1) * 10), Dispz1A(0, i2 + 1))   'PH0のz ' PH0のuseful
                g.DrawLine(MizuiroPen, (i2 * 10), Dispz1A(0, i2), ((i2 + 1) * 10), Dispz1A(0, i2 + 1))   'PH0のz ' PH0のuseful
            End If

            If CheckBox11.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(0, i2), ((i2 + 1) * 10), Dispy1A(0, i2 + 1)) 'PH0のy ' PH0のaverage
                g.DrawLine(LightMizuiroPen, (i2 * 10), Dispy1A(0, i2), ((i2 + 1) * 10), Dispy1A(0, i2 + 1)) 'PH0のy ' PH0のaverage
            End If

            If CheckBox7.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(1, i2), ((i2 + 1) * 10), Dispz1A(1, i2 + 1))   'PH1のz ' PH1のuseful
                g.DrawLine(UsuorangePen, (i2 * 10), Dispz1A(1, i2), ((i2 + 1) * 10), Dispz1A(1, i2 + 1))   'PH1のz ' PH1のuseful
            End If

            If CheckBox10.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(1, i2), ((i2 + 1) * 10), Dispy1A(1, i2 + 1)) 'PH1のy ' PH1のaverage
                g.DrawLine(LightUsuorangePen, (i2 * 10), Dispy1A(1, i2), ((i2 + 1) * 10), Dispy1A(1, i2 + 1)) 'PH1のy ' PH1のaverage
            End If

            If CheckBox6.Checked Then
                'g.DrawLine(RedPen, (i2 * 10), Dispz1A(2, i2), ((i2 + 1) * 10), Dispz1A(2, i2 + 1))   'PH2のz ' PH2のuseful
                g.DrawLine(KimidoriPen, (i2 * 10), Dispz1A(2, i2), ((i2 + 1) * 10), Dispz1A(2, i2 + 1))   'PH2のz ' PH2のuseful
            End If

            If CheckBox9.Checked Then
                'g.DrawLine(GreenPen, (i2 * 10), Dispy1A(2, i2), ((i2 + 1) * 10), Dispy1A(2, i2 + 1)) 'PH2のy ' PH2のaverage
                g.DrawLine(LightKimidoriPen, (i2 * 10), Dispy1A(2, i2), ((i2 + 1) * 10), Dispy1A(2, i2 + 1)) 'PH2のy ' PH2のaverage
            End If

        Next


    End Sub



    '0330 Dim datz(0) As Short
    '0330 Dim daty(0) As Short
    '0330 Dim datx(0) As Short

    Dim datz(2) As Short
    Dim daty(2) As Short
    Dim datx(2) As Short

    Dim disp_recv_str_log As String  ' 2021.2.11   
    Dim fileflg As Int16 = 0
    Dim com_flg As Int16 = 0

    '0330 Dim asciiString_useful As String
    '0330 Dim asciiString_average As String
    '0330 Dim asciiString_diff As String
    Dim asciiString_useful0 As String
    Dim asciiString_useful1 As String
    Dim asciiString_useful2 As String

    Dim asciiString_average0 As String
    Dim asciiString_average1 As String
    Dim asciiString_average2 As String

    Dim asciiString_diff0 As String
    Dim asciiString_diff1 As String
    Dim asciiString_diff2 As String

    Dim file_num As Integer = 0
    Dim expand_flg As Integer = 0

    Private Sub SetTextSafe(textBox As TextBox, text As String)
        If textBox.InvokeRequired Then
            textBox.Invoke(New Action(Of TextBox, String)(AddressOf SetTextSafe), textBox, text)
        Else
            textBox.Text = text
        End If
    End Sub



    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived


        Dim dlgzyx As New DisplayTextAllDelegate(AddressOf DisplayTextAll)
        Dim dlgzyx2 As New DisplayTextAllDelegate(AddressOf DisplayTextAll2)

        '0330 Dim bmp As New Bitmap(PictureBox4.Width, PictureBox4.Height)
        '0330 Dim g As Graphics = Graphics.FromImage(bmp)

        Dim bmp0 As New Bitmap(PictureBox4.Width, PictureBox4.Height)
        Dim g0 As Graphics = Graphics.FromImage(bmp0)
        Dim bmp1 As New Bitmap(PictureBox5.Width, PictureBox5.Height)
        Dim g1 As Graphics = Graphics.FromImage(bmp1)
        Dim bmp2 As New Bitmap(PictureBox6.Width, PictureBox6.Height)
        Dim g2 As Graphics = Graphics.FromImage(bmp2)


        Dim brush As New SolidBrush(Color.Aqua)
        Dim brushB As New SolidBrush(Color.Blue)
        Dim brushRed As New SolidBrush(Color.Red)
        Dim brushGreen As New SolidBrush(Color.Green)
        Dim brushGray As New SolidBrush(Color.LightGray)

        While SerialPort1.BytesToRead > 0
            Dim receivedByte As Byte = SerialPort1.ReadByte()




            buf_chk(133) = buf_chk(132) 'P
            buf_chk(132) = buf_chk(131) 'H
            buf_chk(131) = buf_chk(130) '0
            buf_chk(130) = buf_chk(129) ',
            buf_chk(129) = buf_chk(128) '+-
            buf_chk(128) = buf_chk(127)
            buf_chk(127) = buf_chk(126)
            buf_chk(126) = buf_chk(125)
            buf_chk(125) = buf_chk(124)
            buf_chk(124) = buf_chk(123)
            buf_chk(123) = buf_chk(122)
            buf_chk(122) = buf_chk(121)
            buf_chk(121) = buf_chk(120) ',
            buf_chk(120) = buf_chk(119) '+-
            buf_chk(119) = buf_chk(118)
            buf_chk(118) = buf_chk(117)
            buf_chk(117) = buf_chk(116)
            buf_chk(116) = buf_chk(115)
            buf_chk(115) = buf_chk(114)
            buf_chk(114) = buf_chk(113)
            buf_chk(113) = buf_chk(112)
            buf_chk(112) = buf_chk(111) ',
            buf_chk(111) = buf_chk(110) '+-
            buf_chk(110) = buf_chk(109)
            buf_chk(109) = buf_chk(108)
            buf_chk(108) = buf_chk(107)
            buf_chk(107) = buf_chk(106)
            buf_chk(106) = buf_chk(105)
            buf_chk(105) = buf_chk(104)
            buf_chk(104) = buf_chk(103)
            buf_chk(103) = buf_chk(102) ',
            buf_chk(102) = buf_chk(101) 'bottun
            buf_chk(101) = buf_chk(100) ',


            buf_chk(100) = buf_chk(99) 'P
            buf_chk(99) = buf_chk(98) 'H
            buf_chk(98) = buf_chk(97) '1
            buf_chk(97) = buf_chk(96) ',
            buf_chk(96) = buf_chk(95) '+-
            buf_chk(95) = buf_chk(94)
            buf_chk(94) = buf_chk(93)
            buf_chk(93) = buf_chk(92)
            buf_chk(92) = buf_chk(91)
            buf_chk(91) = buf_chk(90)
            buf_chk(90) = buf_chk(89)
            buf_chk(89) = buf_chk(88)
            buf_chk(88) = buf_chk(87) ',
            buf_chk(87) = buf_chk(86) '+-
            buf_chk(86) = buf_chk(85)
            buf_chk(85) = buf_chk(84)
            buf_chk(84) = buf_chk(83)
            buf_chk(83) = buf_chk(82)
            buf_chk(82) = buf_chk(81)
            buf_chk(81) = buf_chk(80)
            buf_chk(80) = buf_chk(79)
            buf_chk(79) = buf_chk(78) ',
            buf_chk(78) = buf_chk(77) '+-
            buf_chk(77) = buf_chk(76)
            buf_chk(76) = buf_chk(75)
            buf_chk(75) = buf_chk(74)
            buf_chk(74) = buf_chk(73)
            buf_chk(73) = buf_chk(72)
            buf_chk(72) = buf_chk(71)
            buf_chk(71) = buf_chk(70)
            buf_chk(70) = buf_chk(69) ', 
            buf_chk(69) = buf_chk(68) 'button
            buf_chk(68) = buf_chk(67) ',

            buf_chk(67) = buf_chk(66) 'P
            buf_chk(66) = buf_chk(65) 'H
            buf_chk(65) = buf_chk(64) '2
            buf_chk(64) = buf_chk(63) ',
            buf_chk(63) = buf_chk(62) '+-
            buf_chk(62) = buf_chk(61)
            buf_chk(61) = buf_chk(60)
            buf_chk(60) = buf_chk(59)
            buf_chk(59) = buf_chk(58)
            buf_chk(58) = buf_chk(57)
            buf_chk(57) = buf_chk(56)
            buf_chk(56) = buf_chk(55)
            buf_chk(55) = buf_chk(54) ',
            buf_chk(54) = buf_chk(53) '+-
            buf_chk(53) = buf_chk(52)
            buf_chk(52) = buf_chk(51)
            buf_chk(51) = buf_chk(50)
            buf_chk(50) = buf_chk(49)
            buf_chk(49) = buf_chk(48)
            buf_chk(48) = buf_chk(47)
            buf_chk(47) = buf_chk(46)
            buf_chk(46) = buf_chk(45) ',
            buf_chk(45) = buf_chk(44) '+-
            buf_chk(44) = buf_chk(43)
            buf_chk(43) = buf_chk(42)
            buf_chk(42) = buf_chk(41)
            buf_chk(41) = buf_chk(40)
            buf_chk(40) = buf_chk(39)
            buf_chk(39) = buf_chk(38)
            buf_chk(38) = buf_chk(37)
            buf_chk(37) = buf_chk(36) ',
            buf_chk(36) = buf_chk(35) 'bottun
            buf_chk(35) = buf_chk(34) ',

            buf_chk(34) = buf_chk(33) 'P
            buf_chk(33) = buf_chk(32) 'H
            buf_chk(32) = buf_chk(31) '3
            buf_chk(31) = buf_chk(30) ',
            buf_chk(30) = buf_chk(29) '+-
            buf_chk(29) = buf_chk(28)
            buf_chk(28) = buf_chk(27)
            buf_chk(27) = buf_chk(26)
            buf_chk(26) = buf_chk(25)
            buf_chk(25) = buf_chk(24)
            buf_chk(24) = buf_chk(23)
            buf_chk(23) = buf_chk(22)
            buf_chk(22) = buf_chk(21) ',
            buf_chk(21) = buf_chk(20) '+-
            buf_chk(20) = buf_chk(19)
            buf_chk(19) = buf_chk(18)
            buf_chk(18) = buf_chk(17)
            buf_chk(17) = buf_chk(16)
            buf_chk(16) = buf_chk(15)
            buf_chk(15) = buf_chk(14)
            buf_chk(14) = buf_chk(13)
            buf_chk(13) = buf_chk(12) ',
            buf_chk(12) = buf_chk(11) '+-
            buf_chk(11) = buf_chk(10)
            buf_chk(10) = buf_chk(9)
            buf_chk(9) = buf_chk(8)
            buf_chk(8) = buf_chk(7)
            buf_chk(7) = buf_chk(6)
            buf_chk(6) = buf_chk(5)
            buf_chk(5) = buf_chk(4)
            buf_chk(4) = buf_chk(3) ',
            buf_chk(3) = buf_chk(2) 'bottun
            buf_chk(2) = buf_chk(1) ',
            buf_chk(1) = buf_chk(0)
            buf_chk(0) = receivedByte

            'If (buf_chk(95) = &H3A) And (buf_chk(63) = &H3A) And (buf_chk(31) = &H3A) Then
            If (buf_chk(133) = &H50) And (buf_chk(132) = &H48) And (buf_chk(131) = &H30) Then

                data_out_res = 1
                'buf_chk(63) = 0
                'buf_chk(31) = 0

            Else
                data_out_res = 0
            End If

            If (data_out_res = 1) Then
                count_32 = 0
            Else
                count_32 = count_32 + 1
            End If

            If (data_out_res = 1) Then

                'Dim byteArray_useful0() As Byte = {buf_chk(94), buf_chk(93), buf_chk(92), buf_chk(91), buf_chk(90), buf_chk(89), buf_chk(88), buf_chk(87)}
                Dim byteArray_useful0() As Byte = {buf_chk(96), buf_chk(95), buf_chk(94), buf_chk(93), buf_chk(92), buf_chk(91), buf_chk(90), buf_chk(89)}
                asciiString_useful0 = System.Text.Encoding.ASCII.GetString(byteArray_useful0)
                SetTextSafe(TextBox3, asciiString_useful0)
                Dim number_useful0 As Integer = Convert.ToInt32(asciiString_useful0)
                TextBox6.Text = number_useful0.ToString() 'グラフ表示の演算用変数

                'Dim byteArray_useful1() As Byte = {buf_chk(62), buf_chk(61), buf_chk(60), buf_chk(59), buf_chk(58), buf_chk(57), buf_chk(56), buf_chk(55)}
                Dim byteArray_useful1() As Byte = {buf_chk(63), buf_chk(62), buf_chk(61), buf_chk(60), buf_chk(59), buf_chk(58), buf_chk(57), buf_chk(56)}
                asciiString_useful1 = System.Text.Encoding.ASCII.GetString(byteArray_useful1)
                SetTextSafe(TextBox9, asciiString_useful1)
                Dim number_useful1 As Integer = Convert.ToInt32(asciiString_useful1)
                TextBox6.Text = number_useful1.ToString() 'グラフ表示の演算用変数

                Dim byteArray_useful2() As Byte = {buf_chk(30), buf_chk(29), buf_chk(28), buf_chk(27), buf_chk(26), buf_chk(25), buf_chk(24), buf_chk(23)}
                asciiString_useful2 = System.Text.Encoding.ASCII.GetString(byteArray_useful2)
                TextBox12.Text = asciiString_useful2 'GUのテキストボックス用
                Dim number_useful2 As Integer = Convert.ToInt32(asciiString_useful2)
                TextBox6.Text = number_useful2.ToString() 'グラフ表示の演算用変数



                'Dim byteArray_average0() As Byte = {buf_chk(85), buf_chk(84), buf_chk(83), buf_chk(82), buf_chk(81), buf_chk(80), buf_chk(79), buf_chk(78)}
                Dim byteArray_average0() As Byte = {buf_chk(87), buf_chk(86), buf_chk(85), buf_chk(84), buf_chk(83), buf_chk(82), buf_chk(81), buf_chk(80)}
                asciiString_average0 = System.Text.Encoding.ASCII.GetString(byteArray_average0)
                TextBox4.Text = asciiString_average0 'GUのテキストボックス用
                Dim number_average0 As Integer = Convert.ToInt32(asciiString_average0)
                TextBox7.Text = number_average0.ToString() 'グラフ表示の演算用変数

                'Dim byteArray_average1() As Byte = {buf_chk(53), buf_chk(52), buf_chk(51), buf_chk(50), buf_chk(49), buf_chk(48), buf_chk(47), buf_chk(46)}
                Dim byteArray_average1() As Byte = {buf_chk(54), buf_chk(53), buf_chk(52), buf_chk(51), buf_chk(50), buf_chk(49), buf_chk(48), buf_chk(47)}
                asciiString_average1 = System.Text.Encoding.ASCII.GetString(byteArray_average1)
                TextBox10.Text = asciiString_average1 'GUのテキストボックス用
                Dim number_average1 As Integer = Convert.ToInt32(asciiString_average1)
                TextBox7.Text = number_average1.ToString() 'グラフ表示の演算用変数

                Dim byteArray_average2() As Byte = {buf_chk(21), buf_chk(20), buf_chk(19), buf_chk(18), buf_chk(17), buf_chk(16), buf_chk(15), buf_chk(14)}
                asciiString_average2 = System.Text.Encoding.ASCII.GetString(byteArray_average2)
                TextBox13.Text = asciiString_average2 'GUのテキストボックス用
                Dim number_average2 As Integer = Convert.ToInt32(asciiString_average2)
                TextBox7.Text = number_average2.ToString() 'グラフ表示の演算用変数



                'Dim byteArray_diff0() As Byte = {buf_chk(76), buf_chk(75), buf_chk(74), buf_chk(73), buf_chk(72), buf_chk(71), buf_chk(70), buf_chk(69)}
                Dim byteArray_diff0() As Byte = {buf_chk(78), buf_chk(77), buf_chk(76), buf_chk(75), buf_chk(74), buf_chk(73), buf_chk(72), buf_chk(71)}
                asciiString_diff0 = System.Text.Encoding.ASCII.GetString(byteArray_diff0)
                TextBox5.Text = asciiString_diff0 'GUのテキストボックス用
                Dim number_diff0 As Integer = Convert.ToInt32(asciiString_diff0)
                TextBox8.Text = number_diff0.ToString() 'グラフ表示の演算用変数

                'Dim byteArray_diff1() As Byte = {buf_chk(44), buf_chk(43), buf_chk(42), buf_chk(41), buf_chk(40), buf_chk(39), buf_chk(38), buf_chk(37)}
                Dim byteArray_diff1() As Byte = {buf_chk(45),buf_chk(44), buf_chk(43), buf_chk(42), buf_chk(41), buf_chk(40), buf_chk(39), buf_chk(38)}
                asciiString_diff1 = System.Text.Encoding.ASCII.GetString(byteArray_diff1)
                TextBox11.Text = asciiString_diff1 'GUのテキストボックス用
                Dim number_diff1 As Integer = Convert.ToInt32(asciiString_diff1)
                TextBox8.Text = number_diff1.ToString() 'グラフ表示の演算用変数

                Dim byteArray_diff2() As Byte = {buf_chk(12), buf_chk(11), buf_chk(10), buf_chk(9), buf_chk(8), buf_chk(7), buf_chk(6), buf_chk(5)}
                asciiString_diff2 = System.Text.Encoding.ASCII.GetString(byteArray_diff2)
                TextBox14.Text = asciiString_diff2 'GUのテキストボックス用
                Dim number_diff2 As Integer = Convert.ToInt32(asciiString_diff2)
                TextBox8.Text = number_diff2.ToString() 'グラフ表示の演算用変数


                ＇LED on-off0の状態をfileへ書き込み用
                Dim LED_ON_OFF0 As String
                LED_ON_OFF0 = Chr(buf_chk(67))


                'If buf_chk(67) = &H30 Then
                If buf_chk(69) = &H30 Then
                    g0.FillEllipse(brushGray, 0, 0, bmp0.Width, bmp0.Height)
                    g0.DrawEllipse(Pens.Black, 0, 0, bmp0.Width - 1, bmp0.Height - 1)
                    PictureBox4.Image = bmp0
                'ElseIf buf_chk(67) = &H31 Then
                ElseIf buf_chk(69) = &H31 Then
                    'g0.FillEllipse(brushRed, 0, 0, bmp0.Width, bmp0.Height)
                    g0.FillEllipse(brushB, 0, 0, bmp0.Width, bmp0.Height)
                    g0.DrawEllipse(Pens.Black, 0, 0, bmp0.Width - 1, bmp0.Height - 1)
                    PictureBox4.Image = bmp0
                End If

                ＇LED on-off1の状態をfileへ書き込み用
                Dim LED_ON_OFF1 As String
                LED_ON_OFF1 = Chr(buf_chk(35))


                'If buf_chk(35) = &H32 Then
                If buf_chk(36) = &H32 Then
                    g1.FillEllipse(brushGray, 0, 0, bmp1.Width, bmp1.Height)
                    g1.DrawEllipse(Pens.Black, 0, 0, bmp1.Width - 1, bmp1.Height - 1)

                    PictureBox5.Image = bmp1
                'ElseIf buf_chk(35) = &H33 Then
                ElseIf buf_chk(36) = &H33 Then
                    g1.FillEllipse(brushRed, 0, 0, bmp1.Width, bmp1.Height)
                    g1.DrawEllipse(Pens.Black, 0, 0, bmp1.Width - 1, bmp1.Height - 1)
                    PictureBox5.Image = bmp1
                End If

                ＇LED on-off1の状態をfileへ書き込み用
                Dim LED_ON_OFF2 As String
                LED_ON_OFF2 = Chr(buf_chk(3))

                If buf_chk(3) = &H34 Then
                    g2.FillEllipse(brushGray, 0, 0, bmp2.Width, bmp2.Height)
                    g2.DrawEllipse(Pens.Black, 0, 0, bmp2.Width - 1, bmp2.Height - 1)

                    PictureBox6.Image = bmp2
                ElseIf buf_chk(3) = &H35 Then
                    'g2.FillEllipse(brushRed, 0, 0, bmp2.Width, bmp2.Height)
                    g2.FillEllipse(brushGreen, 0, 0, bmp2.Width, bmp2.Height)
                    g2.DrawEllipse(Pens.Black, 0, 0, bmp2.Width - 1, bmp2.Height - 1)
                    PictureBox6.Image = bmp2
                End If


                datz(0) = number_useful0 / 10000
                daty(0) = number_average0 / 10000
                datx(0) = number_diff0 / 10000

                datz(1) = number_useful1 / 10000    '0402 'datz(0) = number_useful1 / 10000
                daty(1) = number_average1 / 10000   '0402 'daty(0) = number_average1 / 10000
                datx(1) = number_diff1 / 10000      '0402 'datx(0) = number_diff1 / 10000

                datz(2) = number_useful2 / 10000    '0402 'datz(0) = number_useful2 / 10000
                daty(2) = number_average2 / 10000   '0402 'daty(0) = number_average2 / 10000
                datx(2) = number_diff2 / 10000      '0402 'datx(0) = number_diff2 / 10000


                If run_flg = 1 Then
                    'count_32 = 0になるのは、0.1秒づつなので、0.1秒毎に、グラフを表示する。
                    'run_flgは、表示ルーチンで処理するので、常時、表示。
                    Me.Invoke(dlgzyx, New Object() {datz, daty, datx}) 'Me.Invoke(dlgzyx, New Object() {datz, daty, datt2})  ' ファイル出力しながら、画面表示するとジャギーになるので、排他制御する                          
                    Me.Invoke(dlgzyx2, New Object() {datz, daty, datx})
                End If

                'ファイル出力処理
                If (fileflg = 1) Then


                    Dim disp_recv_str_log0 As String
                    Dim disp_recv_str_log1 As String
                    Dim disp_recv_str_log2 As String
                    Dim disp_recv_str_log3 As String

                    disp_recv_str_log0 = ""
                    disp_recv_str_log1 = ""
                    disp_recv_str_log2 = ""
                    disp_recv_str_log3 = ""


                    disp_recv_str_log0 = disp_recv_str_log0 & "," & asciiString_useful0 & "," & asciiString_average0 & "," & asciiString_diff0 & "," & LED_ON_OFF0
                    disp_recv_str_log1 = disp_recv_str_log1 & "," & asciiString_useful1 & "," & asciiString_average1 & "," & asciiString_diff1 & "," & LED_ON_OFF1
                    disp_recv_str_log2 = disp_recv_str_log2 & "," & asciiString_useful2 & "," & asciiString_average2 & "," & asciiString_diff2 & "," & LED_ON_OFF2
                    disp_recv_str_log3 = disp_recv_str_log3 & disp_recv_str_log0 & disp_recv_str_log1 & disp_recv_str_log2 & vbCrLf
                    Writer.Write(disp_recv_str_log3)


                End If


            End If



        End While
    End Sub

    'Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
    'Dim sz As Integer = SerialPort1.BytesToRead
    'Dim i As Integer
    'Dim rxdat(sz - 1) As Byte
    '
    '    SerialPort1.Read(rxdat, 0, sz)
    '    Console.WriteLine("受信したバイト数: " & sz.ToString())


    'For i = 0 To sz - 1
    '        RxRingSet(rxdat(i))
    '        'RxRingSet(48)
    '        Console.WriteLine("受信したバイト: " & rxdat(i).ToString())
    'Next

    'End Sub

    'RxRingのwpを更新する
    Public Sub RxRingSet(ByVal dat)

        RxRing.dat(RxRing.wp) = dat

        RxRing.wp += 1
        If RxRing.wp = RxRing.dat.Length Then
            RxRing.wp = 0
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim sz As Integer
        Dim i As Integer

        ComPortChk() '存在するポートを検索する' COMポートリストを定期更新する

        sz = RxRing.wp - RxRing.rp

        If sz < 0 Then
            sz += RxRing.dat.Length
        End If

        If sz > 0 Then
            For i = 0 To sz - 1
                TextBox2.Text &= Chr(RxRing.dat(RxRing.rp))
                RingRpAdd()
            Next

        End If

    End Sub

    'RxRingのrpを更新する
    Private Sub RingRpAdd()

        RxRing.rp += 1
        If RxRing.rp >= RxRing.dat.Length Then
            RxRing.rp = 0
        End If

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        TextBox2.Clear()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        selectComNo = ComboBox1.SelectedIndex
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Dim FileName As String
    Dim Writer As IO.StreamWriter
    Dim Encode As System.Text.Encoding
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        ' RGB値を指定してボタンの背景色を設定（ここでは赤色）
        Dim red As Integer = 155
        Dim green As Integer = 230
        Dim blue As Integer = 253

        'ExecuteCheckBoxChanged()

        If fileflg = 0 Then
            fileflg = 1

            Dim dt1 As DateTime = DateTime.Now

            'disp_recv_str_log = dt1.ToString("yyyy-MM-dd HH時mm分ss秒")
            disp_recv_str_log = dt1.ToString("yyyy.MM.dd HH.mm.ss")

            Encode = System.Text.Encoding.GetEncoding("Shift-JIS")
            'FileName = "G:\NTE\Data_Logger\data\out" & file_num & ".txt"
            'FileName = "D:\NTE\Data_Logger\data\out" & file_num & ".csv"
            'FileName = "out" & file_num & ".csv"
            'FileName = "out" & disp_recv_str_log & ".csv"
            FileName = disp_recv_str_log & ".csv"
            Writer = New IO.StreamWriter(FileName, False, Encode)




            'disp_recv_str_log = dt1.ToString("yyyy/MM/dd HH:mm:ss")

            disp_recv_str_log = disp_recv_str_log & "  ログ開始" & vbCrLf
            Writer.Write(disp_recv_str_log)

            disp_recv_str_log = ""
            ＇disp_recv_str_log = disp_recv_str_log & " " & "," & "     useful" & "," & "   average" & "," & "          diff " & vbCrLf
            '0330 disp_recv_str_log = disp_recv_str_log & " " & "," & "     useful" & "," & "   average" & "," & "          diff " & "," & "          LED " & vbCrLf
            disp_recv_str_log = disp_recv_str_log & " " & "," & "     useful" & "," & "   average" & "," & "          diff " & "," & "          LED " & "," & "    useful1" & "," & "  average1" & "," & "         diff1 " & "," & "         LED1 " & "," & "    useful2" & "," & "  average2" & "," & "         diff2 " & "," & "         LED2 " & vbCrLf

            Writer.Write(disp_recv_str_log)

        Else
            fileflg = 0

            Writer.Close()
        End If

        If fileflg = 1 Then
            Button5.Text = "ファイル書き込み中"
            Button5.BackColor = Color.FromArgb(red, green, blue)
        Else
            Button5.Text = "ファイルclose中"
            Button5.BackColor = Color.White
        End If

    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

    End Sub



    'Dim bmp As New Bitmap(PictureBox4.Width, PictureBox4.Height)
    'Dim g As Graphics = Graphics.FromImage(bmp)

    'Dim brush As New SolidBrush(Color.White)
    'Dim brushB As New SolidBrush(Color.Blue)


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboInit() 'Comboboxを初期化する
        RxRingInit() 'RxRingの初期化を行う
        com_flg = 0
        fileflg = 0
        '0331 Timer1.Enabled = True '定期的なポートチェックは、timerが必要。

        PictureBox4.Width = 50
        PictureBox4.Height = 50

        Dim bmp0 As New Bitmap(PictureBox4.Width, PictureBox4.Height)
        Dim g0 As Graphics = Graphics.FromImage(bmp0)

        PictureBox5.Width = 50
        PictureBox5.Height = 50

        Dim bmp1 As New Bitmap(PictureBox5.Width, PictureBox5.Height)
        Dim g1 As Graphics = Graphics.FromImage(bmp1)

        PictureBox6.Width = 50
        PictureBox6.Height = 50

        Dim bmp2 As New Bitmap(PictureBox6.Width, PictureBox6.Height)
        Dim g2 As Graphics = Graphics.FromImage(bmp2)

        Dim brushGray As New SolidBrush(Color.LightGray)
        Dim brush As New SolidBrush(Color.Aqua)
        Dim brushB As New SolidBrush(Color.Blue)
        Dim brushBlack As New SolidBrush(Color.Blue)

        g0.FillEllipse(brushGray, 0, 0, bmp0.Width, bmp0.Height)
        'add
        g0.DrawEllipse(Pens.Black, 0, 0, bmp0.Width - 1, bmp0.Height - 1)

        PictureBox4.Image = bmp0

        g1.FillEllipse(brushGray, 0, 0, bmp1.Width, bmp1.Height)
        'add
        g1.DrawEllipse(Pens.Black, 0, 0, bmp1.Width - 1, bmp1.Height - 1)

        PictureBox5.Image = bmp1

        g2.FillEllipse(brushGray, 0, 0, bmp2.Width, bmp2.Height)
        'add
        g2.DrawEllipse(Pens.Black, 0, 0, bmp2.Width - 1, bmp2.Height - 1)

        PictureBox6.Image = bmp2


        'PH0
        'Dim g_ph0 As Graphics = Me.CreateGraphics()
        'Dim myPen As New Pen(Color.Blue, 2)
        'g_ph0.DrawLine(myPen, 35, 88, 45, 88)   ' dispay diff

        'Dim g As Graphics = Me.CreateGraphics()
        'Dim g As Graphics = e.Graphics

        'Dim myPen As New Pen(Color.Blue, 2)
        'g.DrawLine(myPen, 10, 10, 200, 200) ' 線を描画

        'g.Dispose()
        'g_ph0.Dispose()

        Me.Invalidate()

    End Sub

    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        'PH0～PH2の右のチェックボックスのラインを表示
        'DIFF_PH0
        ' Graphicsオブジェクトを取得
        Dim g_diff_ph0 As Graphics = e.Graphics
        Dim g_diff_ph1 As Graphics = e.Graphics
        Dim g_diff_ph2 As Graphics = e.Graphics

        Dim g_usefull_ph0 As Graphics = e.Graphics
        Dim g_usefull_ph1 As Graphics = e.Graphics
        Dim g_usefull_ph2 As Graphics = e.Graphics

        Dim g_average_ph0 As Graphics = e.Graphics
        Dim g_average_ph1 As Graphics = e.Graphics
        Dim g_average_ph2 As Graphics = e.Graphics

        ' ペンを作成
        Dim BluePen As New Pen(Color.Blue, 2)   'diff_ph0
        Dim RedPen As New Pen(Color.Red, 2)     'diff_ph1
        Dim GreenPen As New Pen(Color.Green, 2) 'diff_ph2

        Dim MizuiroPen As New Pen(Color.FromArgb(173, 216, 230), 2)   'usefull_ph0 ' 水色
        Dim UsuorangePen As New Pen(Color.FromArgb(255, 200, 150), 2) 'usefull_ph1 ' 薄オレンジ色
        Dim KimidoriPen As New Pen(Color.FromArgb(50, 205, 50), 2)    'usefull_ph2 ' 黄緑色

        Dim LightMizuiroPen As New Pen(Color.FromArgb(173, 216, 230, 50), 2)   'average_ph0 ' 薄い水色
        Dim LightUsuorangePen As New Pen(Color.FromArgb(255, 200, 150, 50), 2) 'average_ph1 ' 薄い薄オレンジ色
        Dim LightKimidoriPen As New Pen(Color.FromArgb(50, 205, 50, 50), 2)    'average_ph2 ' 薄い黄緑色

        ' ラインを描画
        g_diff_ph0.DrawLine(BluePen, 70, 66, 90, 66)
        g_diff_ph1.DrawLine(RedPen, 70, (126 - 22 - 13), 90, (126 - 22 - 13))
        g_diff_ph2.DrawLine(GreenPen, 70, (164 - 22 - 26), 90, (164 - 22 - 26))

        g_usefull_ph0.DrawLine(MizuiroPen, 70, (443 - 22 - 118), 90, (443 - 22 - 118))
        g_usefull_ph1.DrawLine(UsuorangePen, 70, (481 - 22 - 13 - 118), 90, (481 - 22 - 13 - 118))
        g_usefull_ph2.DrawLine(KimidoriPen, 70, (519 - 22 - 26 - 118), 90, (519 - 22 - 26 - 118))

        g_average_ph0.DrawLine(LightMizuiroPen, 70, (625 - 22 - 118 - 60), 90, (625 - 22 - 118 - 60))
        g_average_ph1.DrawLine(LightUsuorangePen, 70, (663 - 22 - 13 - 118 - 60), 90, (663 - 22 - 13 - 118 - 60))
        g_average_ph2.DrawLine(LightKimidoriPen, 70, (701 - 22 - 26 - 118 - 60), 90, (701 - 22 - 26 - 118 - 60))

        ' ペンの後処理
        BluePen.Dispose()
        RedPen.Dispose()
        GreenPen.Dispose()

        MizuiroPen.Dispose()
        UsuorangePen.Dispose()
        KimidoriPen.Dispose()

        LightMizuiroPen.Dispose()
        LightUsuorangePen.Dispose()
        LightKimidoriPen.Dispose()

    End Sub


    Public Class CustomGroupBox
        Inherits GroupBox

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)

            ' カスタム枠線の色とスタイルを指定
            Dim borderColor As Color = Color.Black
            Dim borderThickness As Integer = 1

            ' ペンを作成
            Dim pen As New Pen(borderColor, borderThickness)

            ' 枠線を描画
            e.Graphics.DrawRectangle(pen, 0, 0, Me.Width - 1, Me.Height - 1)
            'e.Graphics.DrawRectangle(pen, 700, 50, 100, 100)
        End Sub
    End Class

    'Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
    Private Async Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown ' 非同期処理をしたら、初期画面でdlgのpictureが表示できた。
        ' フォームが表示された直後に実行される処理
        'DisplayMessage()
        Await Task.Delay(500)
        Me.Invoke(dlg, New Object() {dat})
        Me.Invoke(dlg2, New Object() {dat})
        'Me.PictureBox1.Refresh()
        'Me.PictureBox2.Refresh()



    End Sub

    Public Sub New()
        ' この呼び出しは、Windows フォーム デザイナーで必要です。 
        InitializeComponent()
        ' ダブルバッファリングを有効にする 
        Me.DoubleBuffered = True
    End Sub

    Dim run_flg As Integer = 0


    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            run_flg = 1 'Label1.Text = "チェックされています"
        Else
            run_flg = 0 'Label1.Text = "チェックされていません"
        End If
    End Sub

    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged

    End Sub

    Private Sub TextBox5_TextChanged(sender As Object, e As EventArgs) Handles TextBox5.TextChanged

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

    End Sub

    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs)

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        Dim dlgzyx2still As New DisplayTextAllDelegate(AddressOf DisplayTextAll2still)
        Dim dlgzyxstill As New DisplayTextAllDelegate(AddressOf DisplayTextAllstill)

        If CheckBox2.Checked Then
            expand_flg = 1 'Label1.Text = "チェックされています"
            Label6.Text = "  750000"
            Label7.Text = -250000
            Label8.Text = "  750000"
            Label9.Text = -250000
        Else
            expand_flg = 0 'Label1.Text = "チェックされていません"
            Label6.Text = 1500000
            Label7.Text = -500000
            Label8.Text = 1500000
            Label9.Text = -500000
        End If

        Me.Invoke(dlgzyx2still, New Object() {datz, daty, datx})
        Me.Invoke(dlgzyxstill, New Object() {datz, daty, datx})


    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click

    End Sub

    Private Sub Label11_Click(sender As Object, e As EventArgs) Handles Label11.Click

    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click

    End Sub

    Private Sub Label14_Click(sender As Object, e As EventArgs) Handles Label14.Click

    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label19_Click(sender As Object, e As EventArgs) Handles Label19.Click

    End Sub

    'Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
    '    ' フォームを閉じる処理
    '    ' 特別な処理が不要であれば、何も追加しなくてもOK
    '    ' ここに必要なクリーンアップ処理を追加することも可能です
    'End Sub

    ' PictureBoxの画像リソースを解放するメソッド
    Private Sub DisposePictureBoxImage(pictureBox As PictureBox)
        If pictureBox.Image IsNot Nothing Then
            pictureBox.Image.Dispose()
            pictureBox.Image = Nothing
        End If
    End Sub

    ' フォームが閉じられる際の処理


    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' アプリケーション全体を終了
        ＇run_flg = 0
        ExecuteCheckBoxChanged()
        System.Threading.Thread.Sleep(1000) 'close時のハング対策
        'run_flg = 0
        'System.Threading.Thread.Sleep(500) 'close時のハング対策
        DisposePictureBoxImage(PictureBox1)
        DisposePictureBoxImage(PictureBox2)
        Application.Exit()
    End Sub


    ' CheckedChangedイベントを手動で実行する
    Private Sub ExecuteCheckBoxChanged()
        ' CheckBox1のCheckedChangedイベントを手動で呼び出す
        If run_flg = 1 Then
            CheckBox1.Checked = Not CheckBox1.Checked
            CheckBox1_CheckedChanged(CheckBox1, EventArgs.Empty)
        End If
    End Sub


    ' Form1のFormClosedイベントハンドラーを定義
    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' フォームが閉じられた後に実行する処理
        MessageBox.Show("フォームが閉じられました")
    End Sub

    Private Sub Label20_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label20_Click_1(sender As Object, e As EventArgs) Handles Label20.Click

    End Sub

    Private Sub Label21_Click(sender As Object, e As EventArgs) Handles Label21.Click

    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged

    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged

    End Sub

    Private Sub Label24_Click(sender As Object, e As EventArgs) Handles Label24.Click
        Me.Close()
    End Sub

    Private isDragging As Boolean = False
    Private startPoint As Point = New Point(0, 0)

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown
        If e.Button = MouseButtons.Left Then
            isDragging = True
            startPoint = New Point(e.X, e.Y)
        End If
    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        If isDragging Then
            Dim p As Point = PointToScreen(e.Location)
            Location = New Point(p.X - startPoint.X, p.Y - startPoint.Y)
        End If
    End Sub

    Private Sub Form1_MouseUp(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp
        isDragging = False
    End Sub

    Private Sub Label30_Click(sender As Object, e As EventArgs) Handles Label30.Click

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim args As New FormClosingEventArgs(CloseReason.UserClosing, False)
        Form1_FormClosing(Me, args)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Me.Hide()        ' Form1 を隠す
        TopForm.Show()   ' 親画面を戻す
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub
End Class