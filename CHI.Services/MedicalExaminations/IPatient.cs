using System;

namespace CHI.Services.MedicalExaminations
{
    public interface IPatient
    {
        string Surname { get; set; }
        string Name { get; set; }
        string Patronymic { get; set; }
        DateTime Birthdate { get; set; }
    }
}
