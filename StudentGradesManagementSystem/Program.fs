open System
open System.Windows.Forms
open System.IO
open System.Text.RegularExpressions

type Class = {
    ClassId: int
    ClassName: string
}

type Student = {
    StudentId: int
    StudentName: string
    Grades: int list
}

type Grade = {
    ClassId: int
    StudentId: int
    StudentGrade: int
}

type Statistics = {
    Average: int
    PassRate: float
    FailRate: float
    HighestGrade: int
    LowestGrade: int
}

type UserRole =
    | Admin
    | Viewer

let studentRelativePath = @"..\..\..\data\student.txt"
let studentFullPath = Path.Combine(Directory.GetCurrentDirectory(), studentRelativePath)

let pattern = @"ID: (\d+), NAME: (.+)"

// CRUD
let appendLineAutoIdToFile filePath content =
    try
        let lines = if File.Exists(filePath) then File.ReadAllLines(filePath) |> Array.toList else []
        let nextID =
            match lines |> List.tryLast with
            | Some lastLine when lastLine.StartsWith("ID:") -> 
                let matched = Regex.Match(lastLine, pattern)
                (int matched.Groups.[1].Value) + 1 // next ID
            | _ -> 1
        let fullContent = $"ID: {nextID}, NAME: {content}\r\n"
        File.AppendAllText(filePath, fullContent)
        Ok("File written successfully")
    with
    | :? IOException as ex -> Error($"IO error to {filePath}: {ex.Message}") 
    | ex -> Error($"Unexpected error: {ex.Message}")

let readFileLines filePath =
    try
        let lines = File.ReadLines(filePath)
        Ok(lines)
    with
    | :? FileNotFoundException as ex -> Error($"{ex.Message}")
    | ex -> Error($"{ex.Message}")

// System Title
let systemTitle = new Label(Text = "Student Grades Management System", Top = 10, Left = 300, Width = 500)

///////////////////////////////###########
///////////////////////////////###########

let rec createMainForm userRole =
    let mainForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let manageStudentButton = new Button(Text = "Manage Student", Top = 200, Left = 50, Width = 200)
    let manageCourseButton = new Button(Text = "Manage Course", Top = 200, Left = 300, Width = 200)
    let manageGradesButton = new Button(Text = "Manage Grades", Top = 200, Left = 550, Width = 200)

    let isViewer = userRole = Viewer

    manageStudentButton.Enabled <- not isViewer
    manageCourseButton.Enabled <- not isViewer

    // Events to open child forms
    manageStudentButton.Click.Add(fun _ ->
        let childForm: Form = createManageStudentChildForm mainForm
        mainForm.Hide()
        childForm.ShowDialog() |> ignore
        mainForm.Show()
    )

    manageCourseButton.Click.Add(fun _ ->
        let childForm: Form = createManageCourseChildForm mainForm
        mainForm.Hide()
        childForm.ShowDialog() |> ignore
        mainForm.Show()
    )

    manageGradesButton.Click.Add(fun _ ->
        let childForm: Form = createManageGradesChildForm mainForm
        mainForm.Hide()
        childForm.ShowDialog() |> ignore
        mainForm.Show()
    )

    mainForm.Controls.AddRange [| systemTitle; manageStudentButton; manageCourseButton; manageGradesButton |]
    mainForm

and createManageStudentChildForm mainForm =
    let childForm = new Form(Text = "Manage Students", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    backButton.Click.Add(fun _ -> childForm.Close())

    let studentNameInput = new TextBox(Top = 100, Left = 150, Width = 200)
    let studentNameLabel = new Label(Text = "Student Name:", Top = 100, Left = 50)
    let addStudentButton = new Button(Text = "Add New Student", Top = 200, Left = 50, Width = 100)

    addStudentButton.Click.Add(fun _ ->
        let studentName = studentNameInput.Text
        try
            let folderPath = Path.GetDirectoryName(studentFullPath)
            if not (Directory.Exists(folderPath)) then Directory.CreateDirectory(folderPath) |> ignore

            match appendLineAutoIdToFile studentFullPath studentName with
            | Ok msg -> MessageBox.Show($"Success: {msg}") |> ignore
            | Error err -> MessageBox.Show($"Error: {err}") |> ignore
        with ex -> MessageBox.Show($"An error occurred: {ex.Message}") |> ignore
    )

    childForm.Controls.AddRange [| backButton; studentNameLabel; studentNameInput; addStudentButton |]
    childForm

and createManageCourseChildForm mainForm =
    let childForm = new Form(Text = "Manage Courses", Width = 800, Height = 600)
    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    backButton.Click.Add(fun _ -> childForm.Close())
    childForm.Controls.Add(backButton)
    childForm

and createManageGradesChildForm mainForm =
    let childForm = new Form(Text = "Manage Grades", Width = 800, Height = 600)
    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    backButton.Click.Add(fun _ -> childForm.Close())

    let viewStats = new Button(Text = "View Statistics", Top = 200, Left = 50, Width = 100)
    viewStats.Click.Add(fun _ ->
        MessageBox.Show("Statistics functionality is under construction.") |> ignore
    )

    childForm.Controls.AddRange [| backButton; viewStats |]
    childForm

let createLoginForm () =
    let loginForm = new Form(Text = "Login", Width = 400, Height = 300)
    let roleLabel = new Label(Text = "Select Role:", Top = 50, Left = 50)
    let roleDropdown = new ComboBox(Top = 100, Left = 50, Width = 200)
    roleDropdown.Items.AddRange [| "Admin"; "Viewer" |]

    let loginButton = new Button(Text = "Login", Top = 150, Left = 50, Width = 100)
    loginButton.Click.Add(fun _ ->
        let selectedRole = roleDropdown.SelectedItem :?> string
        match selectedRole with
        | "Admin" -> 
            loginForm.Hide()
            let mainForm = createMainForm Admin
            Application.Run(mainForm)
        | "Viewer" -> 
            loginForm.Hide()
            let mainForm = createMainForm Viewer
            Application.Run(mainForm)
        | _ -> MessageBox.Show("Please select a valid role.") |> ignore
    )

    loginForm.Controls.AddRange [| roleLabel; roleDropdown; loginButton |]
    loginForm

///////////////////////////////###########
///////////////////////////////###########

[<STAThread>]
do
    let loginForm = createLoginForm()
    Application.Run(loginForm)