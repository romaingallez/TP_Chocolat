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
        private Dictionary<string, (Action, string)> adminMenuActions;

        public UserServices()
        {
            // Initialisez le dictionnaire dans le constructeur
            adminMenuActions = new Dictionary<string, (Action, string)>
            {
                ["1"] = (ListArticles, "Afficher la liste des articles"),
                ["2"] = (InputArticle, "Ajouter un article"),
                ["3"] = (GenerateTotalSalesBill, "Générer la facture pour tous les articles vendus"),
                ["4"] = (GenerateBillByBuyer, "Générer la facture par acheteur"),
                ["5"] = (GenerateBillByDate, "Générer la facture par date d'achat"),
                ["6"] = (Logout, "Se déconnecter")
            };
        }
        

        public void HandleAdministrateur()
        {
            var adminService = new AdministrateurFileService();
            var path = "administrateurs.json";
            string username = null;

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

                // Log the creation of a new admin
                ProjetChocolat.Logging.Logger.LogAction(admin.Login, "Création", "nouvel administrateur");
                // On quite a cause d'un bug out ou l'admin n'est pas sauvegardé temps qu'on ne quitte pas le programme (a voir si j'ai le temps de debug)
                return;
            }
            else
            {
                Console.Write("Entrez votre username: ");
                username = Console.ReadLine();

                Console.Write("Entrez votre mot de passe: ");
                var password = Console.ReadLine();

                var admins = adminService.ReadFromFile(path);
                var admin = admins.Find(a => a.Login == username && a.Password == password);

                if (admin == null)
                {
                    Console.WriteLine("Identifiants incorrects.");
                    // Log failed login attempt
                    ProjetChocolat.Logging.Logger.LogAction(username, "Échec de connexion", "administrateur");
                    return;
                }
                else
                {
                    Console.WriteLine("Connexion réussie!");
                    // Log successful login
                    ProjetChocolat.Logging.Logger.LogAction(username, "Connexion", "administrateur");
                }
            }

            // Afficher le menu
            while (true)
            {
                Console.WriteLine("Que voulez-vous faire?");
                foreach (var menuItem in adminMenuActions)
                {
                    Console.WriteLine($"{menuItem.Key}. {menuItem.Value.Item2}");
                }

                var adminChoice = Console.ReadLine();
                if (adminMenuActions.TryGetValue(adminChoice, out var action))
                {
                    action.Item1(); // Exécute l'action associée au choix
                    // Write the username to console
                    Console.WriteLine(username);
                    ProjetChocolat.Logging.Logger.LogAction(username, "Exécuté", action.Item2);
                }
                else
                {
                    Console.WriteLine("Choix invalide.");
                }

                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
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

            // // Wait for any key to be pressed before continuing
            // Console.WriteLine("Appuyez sur une touche pour continuer...");
            // Console.ReadKey();
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
                File.WriteAllText($"BillFor_{buyer.Key}.txt",
                    $"Total des articles vendus à {buyer.Key}: {buyer.Value}€");
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
                File.WriteAllText($"BillFor_{date.Key:yyyy-MM-dd}.txt",
                    $"Total des articles vendus le {date.Key:dd/MM/yyyy}: {date.Value}€");
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
        
        private void Logout()
        {
            Console.WriteLine("Déconnexion réussie!");
            Environment.Exit(0);
        }


        public void HandleUtilisateur()
        {
            var buyerService = new AcheteurFileService();
            var pathAcheteursJson = "acheteurs.json";

            Console.Write("Entrez votre nom: ");
            // Read the username (nom) from console input put in all uppercase
            var enteredUsername = Console.ReadLine().ToUpper();

            List<Acheteur> existingBuyers = new List<Acheteur>();

            if (System.IO.File.Exists(pathAcheteursJson))
            {
                existingBuyers = buyerService.ReadFromFile(pathAcheteursJson);
            }

            Acheteur buyer = existingBuyers.FirstOrDefault(b => b.Nom == enteredUsername);

            if (buyer != null)
            {
                Console.WriteLine($"Bienvenue, {buyer.Prenom} {buyer.Nom}!");

                // Log the addition of an article
                ProjetChocolat.Logging.Logger.LogAction(buyer.Nom, "Connexion", "acheteur existant");
            }
            else
            {
                Console.WriteLine("Création d'un nouvel acheteur.");
                buyer = new Acheteur(); // buyer is declared outside of the else block

                buyer.Nom = enteredUsername;

                Console.Write("Entrez votre prénom: ");
                buyer.Prenom = Console.ReadLine();

                Console.Write("Entrez votre adresse: ");
                buyer.Adresse = Console.ReadLine();

                string phoneNumberInput;
                int phoneNumber;
                do
                {
                    Console.Write("Entrez votre téléphone: ");
                    phoneNumberInput = Console.ReadLine();
                } while (!int.TryParse(phoneNumberInput, out phoneNumber) || phoneNumber < 0);

                buyer.Telephone = phoneNumber;

                buyerService.WriteToFile(pathAcheteursJson, new List<Acheteur> { buyer });

                // Log the creation of the new buyer
                ProjetChocolat.Logging.Logger.LogAction(enteredUsername, "Création", "nouvel acheteur");

                Console.WriteLine($"Bienvenue, {buyer.Prenom} {buyer.Nom}!");
            }


            var articleService = new ArticleFileService();
            var pathArticlesJson = "articles.json";
            List<Article> availableArticles = System.IO.File.Exists(pathArticlesJson)
                ? articleService.ReadFromFile(pathArticlesJson)
                : new List<Article>();
            List<Article> userCart = new List<Article>();

            Console.WriteLine(
                "Début de la commande. Ajoutez des articles, tapez 'F' pour finir ou 'P' pour voir le prix total, 'Q' pour quitter sans valider.");
            // Display available articles to the user
            for (int i = 0; i < availableArticles.Count; i++)
            {
                var article = availableArticles[i];
                Console.WriteLine(
                    $"N°: {i + 1}, Id: {article.Id}, Reference: {article.Reference}, Prix: {article.Prix}");
            }
            
            
            var articleAcheteService = new ArticleAcheteFileService();
            var pathArticleAchetesJson = "articlesAchetes.json";

            List<ArticleAchete> purchasedArticles = new List<ArticleAchete>();
            if (File.Exists(pathArticleAchetesJson))
            {
                purchasedArticles = articleAcheteService.ReadFromFile(pathArticleAchetesJson);
            }
            

            string userInput;
            do
            {
                Console.Write("> "); // Ajout du prompt `>`
                userInput = Console.ReadLine();

                if (userInput.ToUpper() == "F")
                {
                    // Finaliser la commande
                    break;
                }
                else if (userInput.ToUpper() == "P")
                {
                    // Afficher le prix total
                    float total = userCart.Sum(article => article.Prix);
                    Console.WriteLine($"Le prix total de la commande est : {total}€");
                }
                else if (userInput.ToUpper() == "Q")
                {
                    // Restart the program by calling the Main method again
                    Console.WriteLine("Au revoir!");
                    ProjetChocolat.Logging.Logger.LogAction(buyer.Nom, "Déconnexion", "acheteur");
                    return;
                }
                else
                {
                    int articleIndex;
                    if (int.TryParse(userInput, out articleIndex) && articleIndex >= 1 &&
                        articleIndex <= availableArticles.Count)
                    {
                        var articleToAdd = availableArticles[articleIndex - 1]; // Indexes are zero-based, hence the -1
                        userCart.Add(articleToAdd);
                        Console.WriteLine($"Article ajouté : {articleToAdd.Reference} à {articleToAdd.Prix}€");

                        // Log the addition of an article
                        ProjetChocolat.Logging.Logger.LogAction(buyer.Nom, "Ajout d'un", $"{articleToAdd.Reference}");
                        
                        // Add to the purchased articles list
                        var existingArticleAchete = purchasedArticles.FirstOrDefault(a => a.IdAcheteur == buyer.Id && a.IdChocolat == articleToAdd.Id);
                        if (existingArticleAchete != null)
                        {
                            existingArticleAchete.Quantite++;
                        }
                        else
                        {
                            purchasedArticles.Add(new ArticleAchete
                            {
                                IdAcheteur = buyer.Id,
                                IdChocolat = articleToAdd.Id,
                                Quantite = 1,
                                DateAchat = DateTime.Now
                            });
                        }
                        
                        // Save the updated list of purchased articles
                        articleAcheteService.WriteToFile(pathArticleAchetesJson, purchasedArticles);
                    }
                    else
                    {
                        Console.WriteLine("Entrée non valide. Veuillez entrer le numéro de la position de l'article.");
                    }
                }
            } while (true);

            // Génération du récapitulatif de la commande
            GenerateOrderSummary(buyer, userCart); // `buyer` is the newly created buyer if existingBuyer is null
        }

        private void GenerateOrderSummary(Acheteur buyer, List<Article> userCart)
        {
            var directory = $"{buyer.Nom}-{buyer.Prenom}";
            Directory.CreateDirectory(directory); // Créer le dossier s'il n'existe pas

            var fileName = $"{buyer.Nom}-{buyer.Prenom}-{DateTime.Now:dd-MM-yyyy-HH-mm}.txt";
            var filePath = Path.Combine(directory, fileName);

            using (var sw = new StreamWriter(filePath))
            {
                float total = 0;
                foreach (var article in userCart)
                {
                    sw.WriteLine($"Article: {article.Reference}, Prix: {article.Prix}€");
                    total += article.Prix;
                }

                sw.WriteLine($"Prix total: {total}€");
            }

            Console.WriteLine($"Votre commande a été enregistrée dans {filePath}");
        }
    }
}