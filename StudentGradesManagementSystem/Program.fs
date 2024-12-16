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
let courseRelativePath  = @"..\..\..\data\course.txt"
let courseFullPath = Path.Combine(Directory.GetCurrentDirectory(), courseRelativePath)
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
let addGrade studentId courseId grade =
    let content = $"StudentId: {studentId}, CourseId: {courseId}, Grade: {grade}\r\n"
    File.AppendAllText(gradeFullPath, content)


    
let searchForGradesWithId studentId =
    let lines = File.ReadAllLines(gradeFullPath)
    lines
    |> Array.toList
    |> List.filter(fun line -> line.StartsWith($"StudentId: {studentId}"))

let getAvarrageGrades studentId = 
    searchForGradesWithId studentId
    |> Seq.choose (fun line ->
        if line.Contains("Grade: ") then
            line.Substring(line.IndexOf("Grade: ") + 7).Trim() // Extract grade value
            |> float |> Some
        else
            None)
    |> Seq.toList
    |> fun grades ->
        if grades.IsEmpty then
            None // No grades found for the student
        else
            Some (grades |> List.average) // Calculate the average

let getCourseName courseId = 
    File.ReadLines(courseFullPath)
    |> Seq.tryPick (fun line ->
        if line.StartsWith($"ID: {courseId}, NAME: ") then
            Some (line.Split(", NAME: ").[1].Trim())
        else
            None)

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


// Create main form
let rec createMainForm (role: UserRole) : Form =
    let mainForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let manageStudentButton = new Button(Text = "Manage Student", Top = 150, Left = 50, Width = 200)
    let manageCourseButton = new Button(Text = "Manage Course", Top = 150, Left = 300, Width = 200)
    let manageGradesButton = new Button(Text = "Manage Grades", Top = 150, Left = 550, Width = 200)

    let studentIdInput = new TextBox(Top = 240, Left = 150, Width = 200)
    let studentIdLabel = new Label(Text = "Student ID:", Top = 240, Left = 50)

    let searchButton = new Button(Text = "View Grades", Top = 240, Left = 400, Width = 200)
    let viewStats = new Button(Text = "View Statistics", Top = 240, Left = 650, Width = 100)

    // Output area
    let outputBox = new TextBox(Top = 300, Left = 50, Width = 300, Height = 200, Multiline = true, ReadOnly = true)

    // Logout Button
    let logoutButton = new Button(Text = "Logout", Top = 500, Left = 650, Width = 100)
    logoutButton.Anchor <- AnchorStyles.Bottom ||| AnchorStyles.Right


    searchButton.Click.Add (fun _ -> 
        let studentData = 
            let studentId = studentIdInput.Text
            searchForGradesWithId studentId 
            |> List.map (fun line -> 
                let patternCourseId = @"CourseId: (\d+), Grade: (\d+)"
                let grade = Regex.Match(line, patternCourseId).Groups.[2].Value
                let courseId = Regex.Match(line, patternCourseId).Groups.[1].Value
                let courseName = 
                    match getCourseName courseId with 
                    | Some(value) -> value
                    | None -> ""
                sprintf "Student with ID %s has in %s : %s" studentId courseName grade)
            |> String.concat "\r\n"
        
        let avg = 
            match getAvarrageGrades studentIdInput.Text with 
            | Some(value) -> value
            | None -> 0.0
        
        let outputData = sprintf "%s\r\nStudent Averages = %f" studentData avg
        outputBox.Text <- outputData
    )

    viewStats.Click.Add(fun _ -> 
        let lines = File.ReadAllLines(gradeFullPath)
        let studentsGrades = 
            lines
            |> Array.toList
            |> List.map (fun line -> 
                let pattern = @"StudentId: (\d+), CourseId: (\d+), Grade: (\d+)"
                let matched = Regex.Match(line, pattern)
                let studentId = int matched.Groups.[1].Value
                let grade = int matched.Groups.[3].Value
                studentId, grade)
            |> List.groupBy fst
            |> List.map (fun (studentId, grades) -> 
                let studentName = 
                    File.ReadLines(studentFullPath)
                    |> Seq.tryPick (fun line -> 
                        if line.StartsWith($"ID: {studentId}, NAME: ") then 
                            Some(line.Split(", NAME: ").[1].Trim()) 
                        else None)
                    |> Option.defaultValue "Unknown"
                { StudentId = studentId; StudentName = studentName; Grades = grades |> List.map snd })
        
        let stats = calculateClassStatistics studentsGrades
        let statsMessage = 
            $"Average: {stats.Average}%%\nPass Rate: {stats.PassRate}%%\nFail Rate: {stats.FailRate}%%\n" + 
            $"Highest Grade: {stats.HighestGrade}\nLowest Grade: {stats.LowestGrade}"

        MessageBox.Show(statsMessage) |> ignore
    )
    logoutButton.Click.Add(fun _ -> 
            let (loginForm: Form) = createLoginForm()
            mainForm.Hide()
            loginForm.ShowDialog() |> ignore
            mainForm.Close()
        )
    // Admin role buttons for managing students, courses, and grades
    if role = Admin then
        manageStudentButton.Click.Add(fun _ -> 
            let (childForm: Form) = createManageStudentChildForm mainForm
            mainForm.Hide()
            childForm.ShowDialog() |> ignore
            mainForm.Show()
        )
        manageCourseButton.Click.Add(fun _ -> 
            let (childForm: Form) = createManageCourseChildForm mainForm
            mainForm.Hide()
            childForm.ShowDialog() |> ignore
            mainForm.Show()
        )
        manageGradesButton.Click.Add(fun _ -> 
            let (childForm: Form) = createManageGradesChildForm mainForm
            mainForm.Hide()
            childForm.ShowDialog() |> ignore
            mainForm.Show()
        )
    // Add components to the main form based on role
    if role = Admin then
        mainForm.Controls.AddRange[| manageStudentButton; manageCourseButton; manageGradesButton; searchButton; viewStats; studentIdInput; studentIdLabel |]
    else
        mainForm.Controls.AddRange[| searchButton; viewStats; studentIdInput; studentIdLabel |] // Viewer only sees the grades button

    mainForm.Controls.Add(logoutButton) // Add logout button

    mainForm


// Create the child form for managing students
and createManageStudentChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Manage Student", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageStudentTitle = new Label(Text = "Manage Student", Top = 10 , Left = 300, Width = 500)

    let studentNameInput = new TextBox(Top = 100, Left = 150, Width = 200)
    let studentIdInput = new TextBox(Top = 140, Left = 150, Width = 200)
    let studentNameLabel = new Label(Text = "Student Name:", Top = 100, Left = 50)
    let studentIdLabel = new Label(Text = "Student ID:", Top = 140, Left = 50)

    let addStudentButton = new Button(Text = "Add New Student", Top = 200, Left = 50, Width = 100)
    let editStudentButton = new Button(Text = "Edit Student", Top = 200, Left = 200, Width = 100)
    let deleteStudentButton = new Button(Text = "Delete Student", Top = 200, Left = 350, Width = 100)
    let LogoutButton = new Button(Text = "Logout", Top = 500, Left = 350, Width = 100)

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

    childForm.Controls.AddRange[| manageStudentTitle; backButton; studentNameLabel; studentNameInput; studentIdLabel; studentIdInput; 
                                  addStudentButton; editStudentButton; deleteStudentButton |]
    childForm

// Create the child form for course management
and createManageCourseChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Manage Course", Width = 800, Height = 600)
    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageCourseTitle = new Label(Text = "Manage Course", Top = 10 , Left = 300, Width = 500)

    let courseNameInput = new TextBox(Top = 100, Left = 150, Width = 200)
    let courseIdInput = new TextBox(Top = 140, Left = 150, Width = 200)
    let courseNameLabel = new Label(Text = "Course Name:", Top = 100, Left = 50)
    let courseIdLabel = new Label(Text = "Course ID:", Top = 140, Left = 50)

    let addCourseButton = new Button(Text = "Add New Course", Top = 200, Left = 50, Width = 100)
    let editCourseButton = new Button(Text = "Edit Course", Top = 200, Left = 200, Width = 100)
    let deleteCourseButton = new Button(Text = "Delete Course", Top = 200, Left = 350, Width = 100)
    
    backButton.Click.Add(fun _ -> childForm.Close())

     // Add new course functionality
    addCourseButton.Click.Add(fun _ ->
        let studentName = courseNameInput.Text
        match appendLineAutoIdToFile courseFullPath studentName with
        | Ok msg -> MessageBox.Show($"Success: {msg}") |> ignore
        | Error err -> MessageBox.Show($"Error: {err}") |> ignore
    )

    // Edit course functionality
    editCourseButton.Click.Add(fun _ ->
        let courseName = courseNameInput.Text
        let courseId = int courseIdInput.Text
        let lines = File.ReadAllLines(courseFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.map (fun line ->
                if line.StartsWith($"ID: {courseId}") then
                    $"ID: {courseId}, NAME: {courseName}"
                else line)
        File.WriteAllLines(courseFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Course with ID: {courseId} updated successfully") |> ignore
    )

    // Delete course functionality
    deleteCourseButton.Click.Add(fun _ ->
        let courseId = int courseIdInput.Text
        let lines = File.ReadAllLines(courseFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.filter (fun line -> not (line.StartsWith($"ID: {courseId}")))
        File.WriteAllLines(courseFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Course with ID: {courseId} deleted successfully") |> ignore
    )

    childForm.Controls.AddRange[| manageCourseTitle; backButton; courseNameInput; courseIdInput; courseNameLabel; courseIdLabel
                                  addCourseButton; editCourseButton; deleteCourseButton|]
    childForm

// Create child form for managing grades and viewing statistics
and createManageGradesChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Manage Grades", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageGradesTitle = new Label(Text = "Manage Grades", Top = 10 , Left = 300, Width = 500)

    let studentIdInput = new TextBox(Top = 100, Left = 150, Width = 200)
    let studentIdLabel = new Label(Text = "Student ID:", Top = 100, Left = 50)

    let courseIdInput = new TextBox(Top = 140, Left = 150, Width = 200)
    let courseIdLabel = new Label(Text = "Course ID:", Top = 140, Left = 50)

    let gradeInput = new TextBox(Top = 180, Left = 150, Width = 200)
    let gradeLabel = new Label(Text = " Grade:", Top = 180, Left = 50)

    let addGradeButton = new Button(Text = "Add New Grade", Top = 240, Left = 50, Width = 100)
    let editGradeButton = new Button(Text = "Edit Grade", Top = 240, Left = 200, Width = 100)
    let deleteGradeButton = new Button(Text = "Delete Grade", Top = 240, Left = 350, Width = 100)


    backButton.Click.Add(fun _ -> childForm.Close())

    // Add new course functionality
    addGradeButton.Click.Add(fun _ ->
        let studentId = int studentIdInput.Text
        let courseId = int courseIdInput.Text
        let grade = int gradeInput.Text
        addGrade studentId courseId grade
    )
    // Edit course functionality
    editGradeButton.Click.Add(fun _ ->
        let studentId = int studentIdInput.Text
        let courseId = int courseIdInput.Text
        let grade = int gradeInput.Text
        let lines = File.ReadAllLines(gradeFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.map (fun line ->
                if line.StartsWith($"StudentId: {studentId}, ClassId: {courseId}") then
                    $"StudentId: {studentId}, ClassId: {courseId}, Grade: {grade}"
                else line)
        File.WriteAllLines(gradeFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Grade with Course ID: {courseId} AND Student ID: {studentId} updated successfully") |> ignore 
    )

    // Delete course functionality
    deleteGradeButton.Click.Add(fun _ ->
        let studentId = int studentIdInput.Text
        let courseId = int courseIdInput.Text
        let lines = File.ReadAllLines(gradeFullPath) |> Array.toList
        let updatedLines = 
            lines 
            |> List.filter (fun line -> not (line.StartsWith($"StudentId: {studentId}, CourseId: {courseId}")))
        File.WriteAllLines(gradeFullPath, updatedLines |> Array.ofList)
        MessageBox.Show($"Grade with Course ID: {courseId} AND Student ID: {studentId} deleted successfully") |> ignore
    )


    childForm.Controls.AddRange[| manageGradesTitle; backButton; studentIdInput; studentIdLabel; courseIdInput; courseIdLabel; gradeInput;
                                  gradeLabel; addGradeButton; editGradeButton; deleteGradeButton; |]
    childForm

    // Create the login form and role selection form
and createLoginForm() : Form =
    let loginForm = new Form(Text = "Login", Width = 800, Height = 600)

    // Username and Password Labels and Textboxes
    let usernameLabel = new Label(Text = "Username:", Top = 50, Left = 50)
    let passwordLabel = new Label(Text = "Password:", Top = 90, Left = 50)
    let usernameInput = new TextBox(Top = 50, Left = 150, Width = 150)
    let passwordInput = new TextBox(Top = 90, Left = 150, Width = 150, PasswordChar = '*')

    // Buttons
    let loginButton = new Button(Text = "Login", Top = 150, Left = 120, Width = 100)

    // Login validation and form switching
    loginButton.Click.Add(fun _ -> 
        match usernameInput.Text, passwordInput.Text with
        | "admin", "password" -> 
            MessageBox.Show("Login successful, Welcome Admin!") |> ignore
            loginForm.Hide() // Hide the login form

            // Create the main form for Admin and show it
            let mainForm = createMainForm Admin //admin login
            mainForm.ShowDialog() |> ignore

            loginForm.Close() // Close the login form after the main form is closed
        | "viewer", "password" -> 
            MessageBox.Show("Login successful, Welcome Viewer!") |> ignore
            loginForm.Hide() // Hide the login form

            // Create the main form for Viewer and show it
            let mainForm = createMainForm Viewer // This line is placed here for viewer login
            mainForm.ShowDialog() |> ignore

            loginForm.Close() // Close the login form after the main form is closed
        | _ -> 
            MessageBox.Show("Invalid credentials, try again!") |> ignore
    )

    loginForm.Controls.AddRange[| usernameLabel; passwordLabel; usernameInput; passwordInput; loginButton |]
    loginForm




// Run the application
[<STAThread>]
do
    let loginForm = createLoginForm()
    Application.Run(loginForm)