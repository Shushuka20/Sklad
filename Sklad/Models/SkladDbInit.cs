using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Sklad.Models
{
    public class SkladDbInit : DropCreateDatabaseIfModelChanges<SkladContext>
    {
        protected override void Seed(SkladContext context)
        {
            User u1 = new User()
            {
                Login = "Admin",
                Password = "1234",
                Role = "admin"
            };
            User u2 = new User()
            {
                Login = "Manager",
                Password = "1234",
                Role = "manager"
            };
            /**Montaznik m1 = new Montaznik()
            {
                FIO = "Mont",
                Address = "Address",
                MarkAuto = "Mark",
                NumberAuto = "NumbAuto",
                PasportNum = "PaspNum",
                WhereIssued = "lala",
                WhoIssued = "asd",
                Phone = "553535",
                INN = "1111",
                Snils = "555a55"
            };
            context.Montazniks.Add(m1);*/
            context.Users.Add(u1);
            context.Users.Add(u2);       

            base.Seed(context);
        }
    }
}