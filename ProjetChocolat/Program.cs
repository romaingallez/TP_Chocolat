using System;
using ProjetChocolat.Core;
using System.Threading;


namespace ProjetChocolat
{
    class Program
    {
        static void Main(string[] args)
        {

            var userServices = new UserServices();

            userServices.InitializeFiles();
            // Countdown for five second the clear screen.
            
            
            
            Console.WriteLine("Qui êtes-vous? (1: Administrateur, 2: Utilisateur)");
            Console.Write("> ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                userServices.HandleAdministrateur();
                
            }
            else if (choice == "2")
            {
                userServices.HandleUtilisateur();
            }
            else
            {
                Console.WriteLine("Choix invalide.");
                Console.Clear();
            }
        }
    }
}