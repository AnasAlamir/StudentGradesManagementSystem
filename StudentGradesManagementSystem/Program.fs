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

let studentRelativePath  = @"..\..\..\data\student.txt"
let studentFullPath = Path.Combine(Directory.GetCurrentDirectory(), studentRelativePath)

let pattern = @"ID: (\d+), NAME: (.+)"

//crud
let appendLineAutoIdToFile filePath content =
    try
        let lines = if File.Exists(filePath) then File.ReadAllLines(filePath) |> Array.toList else []
        let nextID =
            match lines |> List.tryLast with
            | Some lastLine when lastLine.StartsWith("ID:") -> 
                let matched = Regex.Match(lastLine, pattern)
                (int matched.Groups.[1].Value) + 1 //next id
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



let systemTitle = new Label(Text = "Student Grades Management System", Top = 10 , Left = 300, Width = 500)

///////////////////////////////###########
///////////////////////////////###########

let rec createMainForm () =
    let mainForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let manageStudentButton = new Button(Text = "Manage Student", Top = 200, Left = 50, Width = 200)
    let manageCourseButton = new Button(Text = "Manage Course", Top = 200, Left = 300, Width = 200)
    let manageGreadesButton = new Button(Text = "Manage Greades", Top = 200, Left = 550, Width = 200)

    // Event to open the child form
    manageStudentButton.Click.Add(fun _ ->
        let childForm:Form = createManageStudentChildForm mainForm
        mainForm.Hide() // Hide the main form
        childForm.ShowDialog() |> ignore // Show the child form as a modal dialog
        mainForm.Show() // Show the main form again when the child form is closed
    )

    manageCourseButton.Click.Add(fun _ ->
        let childForm:Form = createManageCourseChildForm mainForm
        mainForm.Hide() // Hide the main form
        childForm.ShowDialog() |> ignore // Show the child form as a modal dialog
        mainForm.Show() // Show the main form again when the child form is closed
    )

    manageGreadesButton.Click.Add(fun _ ->
        let childForm:Form = createManageGradesChildForm mainForm
        mainForm.Hide() // Hide the main form
        childForm.ShowDialog() |> ignore // Show the child form as a modal dialog
        mainForm.Show() // Show the main form again when the child form is closed
    )

    // Add components to the main form
    mainForm.Controls.AddRange[| systemTitle; manageStudentButton; manageCourseButton; manageGreadesButton |]
    mainForm

// Create the child form
and createManageStudentChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)

    let manageStudentTitle = new Label(Text = "Manage Student", Top = 10 , Left = 300, Width = 500)

    let studentNameInput = new TextBox(Top = 100, Left = 150, Width = 200)
    //let studentIdInput = new TextBox(Top = 140, Left = 150, Width = 200)

    let studentNameLabel = new Label(Text = "Student Name:", Top = 100, Left = 50)
    //let studentIdLabel = new Label(Text = "Student ID:", Top = 140, Left = 50)

    let addStudentButton = new Button(Text = "Add New Student", Top = 200, Left = 50, Width = 100)
    let editStudentButton = new Button(Text = "Edit Student", Top = 200, Left = 200, Width = 100)//not finish
    let deleteStudentButton = new Button(Text = "Delete Student", Top = 200, Left = 350, Width = 100)//not finish

    // Event to return to the main form
    backButton.Click.Add(fun _ ->
        childForm.Close() // Close the child form
    )
    addStudentButton.Click.Add (fun _ ->
    
        let studentName = studentNameInput.Text
         
        //let student = { StudentId = studentId; StudentName = studentName }

        try
            
            
        // Ensure the folder exists
            let folderPath = Path.GetDirectoryName(studentFullPath)
            if not (Directory.Exists(folderPath)) then
                Directory.CreateDirectory(folderPath) |> ignore

            match appendLineAutoIdToFile studentFullPath studentName with
            | Ok msg -> MessageBox.Show($"Success: {msg}")  |> ignore
            | Error err -> MessageBox.Show($"Error: {err}") |> ignore

        with
        | ex -> MessageBox.Show($"An error occurred: {ex.Message}") |> ignore
        //outputBox.Text <- "Student added successfully!"
    )

    // Add components to the child form
    childForm.Controls.AddRange[| manageStudentTitle; backButton; studentNameLabel; studentNameInput;
                                  addStudentButton; editStudentButton; deleteStudentButton |]
    childForm

and createManageCourseChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageCourseTitle = new Label(Text = "Manage Course", Top = 10 , Left = 300, Width = 500)
    // Event to return to the main form
    backButton.Click.Add(fun _ ->
        childForm.Close() // Close the child form
    )

    // Add components to the child form
    childForm.Controls.AddRange[| manageCourseTitle; backButton |]
    childForm

and createManageGradesChildForm (mainForm: Form) =
    let childForm = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)

    let backButton = new Button(Text = "Back to Main Form", Top = 50, Left = 50, Width = 200)
    let manageGradesTitle = new Label(Text = "Manage Grades", Top = 10 , Left = 300, Width = 500)
    let viewStats = new Button(Text = "View Statistics", Top = 200, Left = 50, Width = 100)
    // Event to return to the main form
    backButton.Click.Add(fun _ ->
        childForm.Close() // Close the child form
    )
    
    let calculateAverage (grades: int list) =
        if grades.Length > 0 then List.sum grades / grades.Length else 0
    
    let students = [
        { StudentId = 1; StudentName = "John Doe"; Grades = [85; 90; 78] }
        { StudentId = 2; StudentName = "Jane Smith"; Grades = [92; 88; 79] }
        { StudentId = 3; StudentName = "Sam Brown"; Grades = [70; 65; 80] }
    ]
    
    let classStatistics () : Statistics =
        let allGrades = students |> List.collect (fun s -> s.Grades)
        let passCount = allGrades |> List.filter (fun g -> g >= 50) |> List.length
        let failCount = allGrades.Length - passCount
        let highest = if allGrades.Length > 0 then List.max allGrades else 0
        let lowest = if allGrades.Length > 0 then List.min allGrades else 0
    
        {
            Average = calculateAverage allGrades
            PassRate = float passCount / float allGrades.Length * 100.0
            FailRate = float failCount / float allGrades.Length * 100.0
            HighestGrade = highest
            LowestGrade = lowest
        }

    viewStats.Click.Add(fun _ ->
        let stats = classStatistics ()
        let message = 
            $"Average: {stats.Average}\n" +
            $"Pass Rate: {stats.PassRate:F2}%%\n" +
            $"Fail Rate: {stats.FailRate:F2}%%\n" +
            $"Highest Grade: {stats.HighestGrade}\n" +
            $"Lowest Grade: {stats.LowestGrade}"
        MessageBox.Show(message) |> ignore
        )


    // Add components to the child form
    childForm.Controls.AddRange[| manageGradesTitle; backButton; viewStats |]
    childForm

///////////////////////////////###########
///////////////////////////////###########


// Run the application with child
[<STAThread>]
do
    let mainForm = createMainForm ()
    Application.Run(mainForm)
