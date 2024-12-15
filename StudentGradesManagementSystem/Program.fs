open System
open System.Windows.Forms
open System.IO
open System.Text.RegularExpressions

// Define types
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

// File paths and regex pattern
let studentRelativePath  = @"..\..\..\data\student.txt"
let studentFullPath = Path.Combine(Directory.GetCurrentDirectory(), studentRelativePath)
let gradeRelativePath = @"..\..\..\data\grades.txt"
let gradeFullPath = Path.Combine(Directory.GetCurrentDirectory(), gradeRelativePath)
let pattern = @"ID: (\d+), NAME: (.+)"

// Helper functions to read and write data to file
let appendLineAutoIdToFile filePath content =
    try
        let lines = if File.Exists(filePath) then File.ReadAllLines(filePath) |> Array.toList else []
        let nextID =
            match lines |> List.tryLast with
            | Some lastLine when lastLine.StartsWith("ID:") -> 
                let matched = Regex.Match(lastLine, pattern)
                (int matched.Groups.[1].Value) + 1
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
    | :? FileNotFoundException as ex ->  Error($"{ex.Message}")
    | ex -> Error($"{ex.Message}")

// Add grades for a student
let addGrade studentId classId grade =
    let content = $"StudentId: {studentId}, ClassId: {classId}, Grade: {grade}\r\n"
    File.AppendAllText(gradeFullPath, content)

// Calculate statistics (average, pass/fail rate, highest/lowest grade)
let calculateClassStatistics (students: Student list) =
    let allGrades = students |> List.collect (fun s -> s.Grades)
    let passCount = allGrades |> List.filter (fun g -> g >= 50) |> List.length
    let failCount = allGrades.Length - passCount
    let highest = if allGrades.Length > 0 then List.max allGrades else 0
    let lowest = if allGrades.Length > 0 then List.min allGrades else 0

    {
        Average = if allGrades.Length > 0 then List.sum allGrades / allGrades.Length else 0
        PassRate = float passCount / float allGrades.Length * 100.0
        FailRate = float failCount / float allGrades.Length * 100.0
        HighestGrade = highest
        LowestGrade = lowest
    }

// ** Child Forms Definitions (above createMainForm) **

// Create the child form for managing students
let createManageStudentChildForm (mainForm: Form) (role: UserRole) =
    let childForm = new Form(Text = "Manage Student", Width = 800, Height = 600)
    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let studentNameInput = new TextBox(Top = 100, Left = 150, Width = 200)
    let studentIdInput = new TextBox(Top = 140, Left = 150, Width = 200)
    let studentNameLabel = new Label(Text = "Student Name:", Top = 100, Left = 50)
    let studentIdLabel = new Label(Text = "Student ID:", Top = 140, Left = 50)

    let addStudentButton = new Button(Text = "Add New Student", Top = 200, Left = 50, Width = 100)
    let editStudentButton = new Button(Text = "Edit Student", Top = 200, Left = 200, Width = 100)
    let deleteStudentButton = new Button(Text = "Delete Student", Top = 200, Left = 350, Width = 100)

    // Role check for Viewer
    if role = Viewer then
        addStudentButton.Enabled <- false
        editStudentButton.Enabled <- false
        deleteStudentButton.Enabled <- false

    // Event to go back to the main form
    backButton.Click.Add(fun _ -> childForm.Close())

    // Add new student functionality
    addStudentButton.Click.Add(fun _ -> 
        let studentName = studentNameInput.Text
        match appendLineAutoIdToFile studentFullPath studentName with
        | Ok msg -> MessageBox.Show($"Success: {msg}") |> ignore
        | Error err -> MessageBox.Show($"Error: {err}") |> ignore
    )

    // Edit student functionality
    editStudentButton.Click.Add(fun _ -> 
        let studentName = studentNameInput.Text
        let studentId = int studentIdInput.Text
        let lines = File.ReadAllLines(studentFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.map (fun line -> 
                if line.StartsWith($"ID: {studentId}") then
                    $"ID: {studentId}, NAME: {studentName}"
                else line)
        File.WriteAllLines(studentFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Student with ID: {studentId} updated successfully") |> ignore
    )

    // Delete student functionality
    deleteStudentButton.Click.Add(fun _ -> 
        let studentId = int studentIdInput.Text
        let lines = File.ReadAllLines(studentFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.filter (fun line -> not (line.StartsWith($"ID: {studentId}")))
        File.WriteAllLines(studentFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Student with ID: {studentId} deleted successfully") |> ignore
    )

    childForm.Controls.AddRange[| backButton; studentNameLabel; studentNameInput; studentIdLabel; studentIdInput; 
                                  addStudentButton; editStudentButton; deleteStudentButton |]
    childForm

// Create the child form for managing courses
let createManageCourseChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Manage Course", Width = 800, Height = 600)
    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageCourseTitle = new Label(Text = "Manage Course", Top = 10 , Left = 300, Width = 500)
    
    backButton.Click.Add(fun _ -> childForm.Close())
    childForm.Controls.AddRange[| manageCourseTitle; backButton |]
    childForm

// Create child form for managing grades and viewing statistics
let createManageGradesChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Manage Grades", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageGradesTitle = new Label(Text = "Manage Grades", Top = 10 , Left = 300, Width = 500)
    let viewStats = new Button(Text = "View Statistics", Top = 200, Left = 50, Width = 100)

    backButton.Click.Add(fun _ -> childForm.Close())

    // View class statistics
    viewStats.Click.Add(fun _ -> 
        let students = [
            {StudentId = 1; StudentName = "Alice"; Grades = [60; 70; 80]}
            {StudentId = 2; StudentName = "Bob"; Grades = [50; 60; 65]}
        ]

        let stats = calculateClassStatistics students
        let statsMessage = 
            $"Average: {stats.Average}%%\nPass Rate: {stats.PassRate}%%\nFail Rate: {stats.FailRate}%%\n" +
            $"Highest Grade: {stats.HighestGrade}\nLowest Grade: {stats.LowestGrade}"

        MessageBox.Show(statsMessage) |> ignore
    )

    childForm.Controls.AddRange[| manageGradesTitle; backButton; viewStats |]
    childForm

// Create main form based on role
let rec createMainForm (role: UserRole) =
    let mainForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let manageStudentButton = new Button(Text = "Manage Student", Top = 200, Left = 50, Width = 200)
    let manageCourseButton = new Button(Text = "Manage Course", Top = 200, Left = 300, Width = 200)
    let manageGradesButton = new Button(Text = "Manage Grades", Top = 200, Left = 550, Width = 200)

    if role = Admin then
        manageStudentButton.Click.Add(fun _ -> 
            let childForm: Form = createManageStudentChildForm mainForm role
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

    if role = Admin then
        mainForm.Controls.AddRange[| manageStudentButton; manageCourseButton; manageGradesButton |]
    else
        mainForm.Controls.AddRange[| manageGradesButton |]  // Viewer only sees grades button

    mainForm

// Create the login form and role selection form
let createLoginForm() =
    let loginForm = new Form(Text = "Login", Width = 800, Height = 600)
    let usernameLabel = new Label(Text = "Username:", Top = 50, Left = 50)
    let passwordLabel = new Label(Text = "Password:", Top = 90, Left = 50)
    let usernameInput = new TextBox(Top = 50, Left = 150, Width = 150)
    let passwordInput = new TextBox(Top = 90, Left = 150, Width = 150, PasswordChar = '*')
    let loginButton = new Button(Text = "Login", Top = 150, Left = 120)

    loginButton.Click.Add(fun _ -> 
        if usernameInput.Text = "admin" && passwordInput.Text = "password" then
            MessageBox.Show("Login successful, Welcome Admin!") |> ignore
            loginForm.Hide() // Hide the login form
            let mainForm = createMainForm Admin
            mainForm.ShowDialog() |> ignore // Show the main form as a dialog
            loginForm.Close() // Close the login form after the main form is closed
        elif usernameInput.Text = "viewer" && passwordInput.Text = "password" then
            MessageBox.Show("Login successful, Welcome Viewer!") |> ignore
            loginForm.Hide() // Hide the login form
            let mainForm = createMainForm Viewer
            mainForm.ShowDialog() |> ignore // Show the main form as a dialog
            loginForm.Close() // Close the login form after the main form is closed
        else
            MessageBox.Show("Invalid credentials, try again!") |> ignore
    )

    loginForm.Controls.AddRange[| usernameLabel; passwordLabel; usernameInput; passwordInput; loginButton |]
    loginForm

[<STAThread>]
do
    let loginForm = createLoginForm()
    Application.Run(loginForm)  // Only run the application once with the login form
