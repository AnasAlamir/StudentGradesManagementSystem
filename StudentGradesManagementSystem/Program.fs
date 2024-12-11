open System
open System.Windows.Forms

type Class = {
    ClassId: int
    ClassName: string
}

type Student = {
    StudentId: int
    StudentName: string
}

type Grade = {
    ClassId: int
    StudentId: int
    StudentGrade: int
}

let class_pl3 :Class = { ClassId = 1; ClassName = "pl3" }
let student_anas :Student = { StudentName = "anas"; StudentId = 15 }
let pl3_anas_grade :Grade = { ClassId = 1; StudentId = 15; StudentGrade = 90 }


printfn $"n={student_anas.StudentName}"

//Class
//classId className
//  1           pl3
//  2           os
//  3           ai

//Student
//studentId   studentName
//  1             anas
//  2             samy


//grade
//classId    studentId   studentGrade
//  1             2           90
//  1             1           85
//  2             1           75
//  2             2           87
//  3             1           99  

let mainForm =
    let form = new Form(Text = "Student Grades Management", Width = 600, Height = 400)
    
    let addButton = new Button(Text = "Add Student", Dock = DockStyle.Top)
    addButton.Click.Add(fun _ -> 
        // Open Add Student form 
        MessageBox.Show("Student Added!") |> ignore
        )

    let viewButton = new Button(Text = "View Statistics", Dock = DockStyle.Top)
    viewButton.Click.Add(fun _ -> 
        // Show statistics in a message box
        MessageBox.Show("Statistics viewed") |> ignore
    )

    form.Controls.Add(viewButton)
    form.Controls.Add(addButton)
    form

[<EntryPoint>]
let main argv =
   Application.EnableVisualStyles()
   Application.Run(mainForm)
   0