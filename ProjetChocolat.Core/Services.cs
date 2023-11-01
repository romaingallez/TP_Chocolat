using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjetChocolat.FileServices;
using ProjetChocolat.Models;
using ProjetChocolat.ListManagement;

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
            
            
            while (true)  // To continually serve the admin until they choose to exit
            {
                Console.WriteLine("Que voulez-vous faire?");
                Console.WriteLine("0. Afficher la liste des articles");
                Console.WriteLine("1. Ajouter un article");
                Console.WriteLine("2. Générer la facture pour tous les articles vendus");
                Console.WriteLine("3. Générer la facture par acheteur");
                Console.WriteLine("4. Générer la facture par date d'achat");
                Console.WriteLine("5. Se déconnecter");

                var adminChoice = Console.ReadLine();

                switch (adminChoice)
                {
                    case "0":
                        ListArticles();
                        break;
                    case "1":
                        InputArticle();
                        break;
                    case "2":
                        GenerateTotalSalesBill();
                        break;
                    case "3":
                        GenerateBillByBuyer();
                        break;
                    case "4":
                        GenerateBillByDate();
                        break;
                    case "5":
                        Console.WriteLine("Déconnexion réussie!");
                        return; // Exit the loop and method
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
            
        }
        
        private void ListArticles()
        {
            var articleService = new ArticleFileService();
            var path = "articles.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Aucun article trouvé.");
                return;
            }

            var articles = articleService.ReadFromFile(path);

            foreach (var article in articles)
            {
                Console.WriteLine($"Id: {article.Id}, Reference: {article.Reference}, Prix: {article.Prix}");
            }
            // Wait for any key to be pressed before continuing
            Console.WriteLine("Appuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
        
        private void InputArticle()
        {
            var articleService = new ArticleFileService();
            var path = "articles.json";

            var articles = System.IO.File.Exists(path) ? articleService.ReadFromFile(path) : new List<Article>();

            var newArticle = new Article
            {
                Id = Guid.NewGuid()
            };

            Console.Write("Entrez la référence de l'article: ");
            newArticle.Reference = Console.ReadLine();

            Console.Write("Entrez le prix de l'article: ");
            newArticle.Prix = float.Parse(Console.ReadLine()); // TODO: Handle invalid inputs

            articles.Add(newArticle);
            articleService.WriteToFile(path, articles);

            Console.WriteLine("Article ajouté avec succès!");
        }
        
        private void GenerateTotalSalesBill()
        {
            // This will involve getting all sold articles and summing up.
            var soldArticleService = new ArticleAcheteFileService();
            var path = "articlesAchetes.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Aucun article vendu trouvé.");
                return;
            }

            var soldArticles = soldArticleService.ReadFromFile(path);

            float totalAmount = 0;
            foreach (var soldArticle in soldArticles)
            {
                // Assuming the price of the article remains same. Otherwise, we'd need to fetch the actual price from articles.json
                totalAmount += soldArticle.Quantite * (GetArticlePriceById(soldArticle.IdChocolat));
            }

            File.WriteAllText("TotalSalesBill.txt", $"Total des articles vendus: {totalAmount}€");
            Console.WriteLine("Facture créée: TotalSalesBill.txt");
        }
        
        private void GenerateBillByBuyer()
        {
            var soldArticleService = new ArticleAcheteFileService();
            var path = "articlesAchetes.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Aucun article vendu trouvé.");
                return;
            }

            var soldArticles = soldArticleService.ReadFromFile(path);

            var buyersSales = new Dictionary<Guid, float>();

            foreach (var soldArticle in soldArticles)
            {
                var amount = soldArticle.Quantite * (GetArticlePriceById(soldArticle.IdChocolat));
                if (buyersSales.ContainsKey(soldArticle.IdAcheteur))
                {
                    buyersSales[soldArticle.IdAcheteur] += amount;
                }
                else
                {
                    buyersSales[soldArticle.IdAcheteur] = amount;
                }
            }

            foreach (var buyer in buyersSales)
            {
                File.WriteAllText($"BillFor_{buyer.Key}.txt", $"Total des articles vendus à {buyer.Key}: {buyer.Value}€");
            }

            Console.WriteLine("Factures créées pour chaque acheteur.");
        }
        
        private void GenerateBillByDate()
        {
            var soldArticleService = new ArticleAcheteFileService();
            var path = "articlesAchetes.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Aucun article vendu trouvé.");
                return;
            }

            var soldArticles = soldArticleService.ReadFromFile(path);

            var dateSales = new Dictionary<DateTime, float>();

            foreach (var soldArticle in soldArticles)
            {
                var amount = soldArticle.Quantite * (GetArticlePriceById(soldArticle.IdChocolat));
                if (dateSales.ContainsKey(soldArticle.DateAchat))
                {
                    dateSales[soldArticle.DateAchat] += amount;
                }
                else
                {
                    dateSales[soldArticle.DateAchat] = amount;
                }
            }

            foreach (var date in dateSales)
            {
                File.WriteAllText($"BillFor_{date.Key:yyyy-MM-dd}.txt", $"Total des articles vendus le {date.Key:dd/MM/yyyy}: {date.Value}€");
            }

            Console.WriteLine("Factures créées pour chaque date.");
        }
        
        
        private float GetArticlePriceById(Guid id)
        {
            var articleService = new ArticleFileService();
            var path = "articles.json";
            var articles = articleService.ReadFromFile(path);
            var article = articles.Find(a => a.Id == id);

            return article?.Prix ?? 0;
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
    
            Console.Write("Entrez votre username: ");
            var enteredUsername = Console.ReadLine();

            List<Acheteur> existingBuyers = new List<Acheteur>();

            if (System.IO.File.Exists(path))
            {
                existingBuyers = buyerService.ReadFromFile(path);  // Supposons que ReadFromFile renvoie une liste d'Acheteur.
            }

            var existingBuyer = existingBuyers.FirstOrDefault(b => b.Nom == enteredUsername);

            if (existingBuyer != null)
            {
                Console.WriteLine($"Bienvenue, {existingBuyer.Prenom} {existingBuyer.Nom}!");
                
            }
            else
            {
                Console.WriteLine("Création d'un nouvel acheteur.");
                var buyer = new Acheteur();

                buyer.Nom = enteredUsername;

                Console.Write("Entrez votre nom: ");
                buyer.Nom = Console.ReadLine();

                Console.Write("Entrez votre prénom: ");
                buyer.Prenom = Console.ReadLine();

                Console.Write("Entrez votre adresse: ");
                buyer.Adresse = Console.ReadLine();

                Console.Write("Entrez votre téléphone: ");
                // TODO: Handle invalid inputs.
                buyer.Telephone = int.Parse(Console.ReadLine());  

                buyerService.WriteToFile(path, new List<Acheteur> { buyer });
                
                Console.WriteLine($"Bienvenue, {buyer.Prenom} {buyer.Nom}!");
            }
        }
        
        

    }
    
}
