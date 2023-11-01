using System;
using ProjetChocolat.Core;

namespace ProjetChocolat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Chocolate is good!");

            var userServices = new UserServices();

            Console.WriteLine("Qui êtes-vous? (1: Administrateur, 2: Utilisateur)");
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
            }
        }
    }
}