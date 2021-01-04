export class client
{
    CreatedBy: number;
    CreatedOn: Date;
    MachineName: string;
    UpdatedBy: number;
    UpdatedOn: Date;
    Aadhar: string;
    ClientType:string;
    DOB:Date;
    Name:string;
    FatherName:string;
    Gender:string;
    ID: number;
    ImageData: string;
    ImagePath: string;
    IsDeleted: boolean;
    IsMarried: boolean;
    MarriageAnniversary:Date;
    Occupation:string;
    PAN: string;
    PlaceOfBirth:string;
    ResiStatus:string;
   /* CreatedByUserName: string;
    FirstName: string;
    userId: number;
    */
}
export  class employees
{
    id:string;
    employee_name:string;
    employee_salary:string;
    employee_age:string;
    profile_image:string;
}

export class user
{
    Id: number;
    IsDeleted:boolean;
    FirstName: string;
    LastName: string;
    Password: string;
    RoleId: number;
    UpdatedByUserName:string;
    UserName: string;
}
