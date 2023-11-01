using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjetChocolat.FileServices;
using ProjetChocolat.Models;

namespace ProjetChocolat.Core
{
    public class UserServices
    {
        public void HandleAdministrateur()
        {
            var adminService = new AdministrateurFileService();
            var path = "administrateurs.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Création d'un nouvel administrateur.");
                var admin = new Administrateur();

                Console.Write("Entrez votre username: ");
                admin.Login = Console.ReadLine();

                do
                {
                    Console.Write("Entrez votre mot de passe (6 caractères alphanumériques et 1 caractère spécial): ");
                    admin.Password = Console.ReadLine();
                } while (!IsValidPassword(admin.Password));

                adminService.WriteToFile(path, new List<Administrateur> { admin });
            }
            else
            {
                Console.Write("Entrez votre username: ");
                var username = Console.ReadLine();

                Console.Write("Entrez votre mot de passe: ");
                var password = Console.ReadLine();

                var admins = adminService.ReadFromFile(path);
                var admin = admins.Find(a => a.Login == username && a.Password == password);

                if (admin == null)
                {
                    Console.WriteLine("Identifiants incorrects.");
                }
                else
                {
                    Console.WriteLine("Connexion réussie!");
                }
            }
        }

        public bool IsValidPassword(string password)
        {
            var regex = new Regex(@"^(?=.*[a-zA-Z0-9].{6,})(?=.*[^a-zA-Z0-9])");
            return regex.IsMatch(password);
        }

        public void HandleUtilisateur()
        {
            var buyerService = new AcheteurFileService();
            var path = "acheteurs.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Création d'un nouvel acheteur.");
                var buyer = new Acheteur();

                Console.Write("Entrez votre username: ");
                buyer.Nom = Console.ReadLine();

                Console.Write("Entrez votre nom: ");
                buyer.Nom = Console.ReadLine();

                Console.Write("Entrez votre prénom: ");
                buyer.Prenom = Console.ReadLine();

                Console.Write("Entrez votre adresse: ");
                buyer.Adresse = Console.ReadLine();

                Console.Write("Entrez votre téléphone: ");
                buyer.Telephone = int.Parse(Console.ReadLine());  // TODO: Handle invalid inputs.

                buyerService.WriteToFile(path, new List<Acheteur> { buyer });
            }
            else
            {
                Console.WriteLine("L'utilisateur existe déjà.");
            }
        }
    }
}
