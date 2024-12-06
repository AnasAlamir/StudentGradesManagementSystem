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


// Create a new form
//let createForm () =
//    let form = new Form(Text = "Hello, F# Windows Forms!", Width = 400, Height = 300)

//    // Create a button
//    let button = new Button(Text = "Click Me!", Dock = DockStyle.Fill)

//    // Button click event handler
//    button.Click.Add(fun _ -> MessageBox.Show("Hello, World!") |> ignore)

//    // Add the button to the form
//    form.Controls.Add(button)

//    form

//[<EntryPoint>]
//let main argv =
//    // Create and run the form
//    Application.EnableVisualStyles()
//    Application.Run(createForm())
//    0
