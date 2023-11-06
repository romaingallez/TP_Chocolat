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
        // Ajoutez cette nouvelle méthode dans la classe UserServices
        public void InitializeFiles()
        {
            //Create config folder if it doesn't exist
            if (!System.IO.Directory.Exists("config"))
            {
                System.IO.Directory.CreateDirectory("config");
            }
            // Liste de tous les chemins de fichier nécessaires à votre application
            var filePaths = new List<string>
            {
                "config/administrateurs.json",
                "config/articles.json",
                "config/articlesAchetes.json",
                "config/acheteurs.json"
            };

            // Passez en revue chaque chemin de fichier et créez le fichier s'il n'existe pas
            foreach (var path in filePaths)
            {
                try
                {
                    if (!System.IO.File.Exists(path))
                    {
                        System.IO.File.WriteAllText(path, "[]");
                        Console.WriteLine($"Fichier {path} créé.");
                        ProjetChocolat.Logging.Logger.LogAction("System", "Création", $"fichier {path}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Impossible de créer le fichier {path}: {ex.Message}");
                    ProjetChocolat.Logging.Logger.LogAction("System", "Erreur", $"Impossible de créer le fichier {path}: {ex.Message}");
                }
            }
            
            for (int i = 5; i > 0; i--)
            {
                Console.Write($"\rDémarrage dans {i} secondes.   "); // Overwrite with extra spaces
                Thread.Sleep(1000);
            }
            Console.Write("\rLancement!               "); // Clean up the line after the countdown is complete

            
            Console.Clear();
        }

            // Gère le processus de connexion ou de création d'un nouvel administrateur
        public void HandleAdministrateur()
        {
            var adminService = new AdministrateurFileService();
            var path = "config/administrateurs.json";
            string username = null;

            // Vérifie si le fichier des administrateurs existe déjà
            if (!File.Exists(path))
            {
                Console.WriteLine("Le fichier des administrateurs n'existe pas.");
            }
            else
            {
                
                // Read the config/administrateurs.json file and check if there are any existing admins
                var admins = adminService.ReadFromFile(path);
                
                // If there are no existing admins, create a new one
                if (admins.Count == 0)
                {
                    Console.WriteLine("Création d'un nouvel administrateur.");
                    var admin = new Administrateur();

                    // Demande et stocke le nom d'utilisateur de l'administrateur
                    Console.Write("Entrez votre username: ");
                    Console.Write("> ");
                    admin.Login = Console.ReadLine();
                    username = admin.Login;

                    // Demande et valide le mot de passe de l'administrateur
                    do
                    {
                        Console.Write("Entrez votre mot de passe (6 caractères alphanumériques et 1 caractère spécial): ");
                        Console.Write("> ");
                        admin.Password = Console.ReadLine();
                    }
                    while (!IsValidPassword(admin.Password));

                    // Enregistre le nouvel administrateur dans le fichier
                    adminService.WriteToFile(path, new List<Administrateur> { admin });
                    ProjetChocolat.Logging.Logger.LogAction(admin.Login, "Création", "nouvel administrateur");
                }
                else
                {
                    // If there are existing admins, ask for the username and password
                    Console.Clear();
                    var adminMessage = "Le compte administrateur existe déjà \nConnexion administrateur";
                    Console.WriteLine(new string('-', adminMessage.Length));
                    Console.WriteLine(adminMessage);
                    var usernameAsk = "Entrez votre username: "; 
                    Console.WriteLine(new string('-', usernameAsk.Length));
                    Console.WriteLine(usernameAsk);
                    Console.Write("> ");
                    username = Console.ReadLine();
                    var passwordAsk = "Entrez votre mot de passe: ";
                    Console.WriteLine(passwordAsk);
                    Console.WriteLine(new string('-', usernameAsk.Length));
                    
                    Console.Write("> ");
                    var password = Console.ReadLine();

                    // Find the admin with the matching username and password
                    var admin = admins.Find(a => a.Login == username && a.Password == password);

                    // If no admin is found, the credentials are invalid
                    if (admin == null)
                    {
                        Console.WriteLine("Identifiants incorrects.");
                        ProjetChocolat.Logging.Logger.LogAction(username, "Échec de connexion", "administrateur");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Connexion réussie!");
                        ProjetChocolat.Logging.Logger.LogAction(username, "Connexion", "administrateur");
                    }
                }
            }

            Console.Clear();
            // Menu après connexion
            while (true)
            {
                Console.WriteLine("Que voulez-vous faire?");
                // Affiche les options disponibles pour l'administrateur
                Console.WriteLine("1. Afficher la liste des articles");
                Console.WriteLine("2. Ajouter un article");
                Console.WriteLine("3. Générer la facture pour tous les articles vendus");
                Console.WriteLine("4. Générer la facture par acheteur");
                Console.WriteLine("5. Générer la facture par date d'achat");
                Console.WriteLine("6. Se déconnecter");
                
                
                Console.Write("> ");
                var adminChoice = Console.ReadLine();
                

                switch (adminChoice)
                {
                    case "1":
                        ListArticles();
                        ProjetChocolat.Logging.Logger.LogAction(username, "Liste", "articles");
                        break;
                    case "2":
                        InputArticle(username);
                        break;
                    case "3":
                        GenerateTotalSalesBill();
                        ProjetChocolat.Logging.Logger.LogAction(username, "Généré", "facture totale des ventes");
                        break;
                    case "4":
                        GenerateBillByBuyer();
                        ProjetChocolat.Logging.Logger.LogAction(username, "Généré", "facture par acheteur");
                        break;
                    case "5":
                        GenerateBillByDate();
                        ProjetChocolat.Logging.Logger.LogAction(username, "Généré", "facture par date d'achat");
                        break;
                    case "6":
                        Console.WriteLine("Déconnexion réussie!");
                        ProjetChocolat.Logging.Logger.LogAction(username, "Déconnexion", "administrateur");
                        return;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }

                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
        }
        private void ListArticles()
        {
            var articleService = new ArticleFileService();
            var path = "config/articles.json";

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
            
        }

        private void InputArticle(string username)
        {
            Console.Clear();
            var articleService = new ArticleFileService();
            var path = "config/articles.json";

            var articles = System.IO.File.Exists(path) ? articleService.ReadFromFile(path) : new List<Article>();

            var newArticle = new Article
            {
                Id = Guid.NewGuid()
            };

            Console.Write("Entrez la référence de l'article: ");
            Console.Write("> ");
            newArticle.Reference = Console.ReadLine();

            Console.Write("Entrez le prix de l'article: ");
            Console.Write("> ");
            newArticle.Prix = float.Parse(Console.ReadLine()); // TODO: Handle invalid inputs

            articles.Add(newArticle);
            articleService.WriteToFile(path, articles);
            
            // Log the addition of an article by the admin
            ProjetChocolat.Logging.Logger.LogAction(user:username, "Ajout de", $"{newArticle.Reference}");
            Console.WriteLine("Article ajouté avec succès!");
            Console.Clear();
        }

        private void GenerateTotalSalesBill()
        {
            Console.Clear();
            // This will involve getting all sold articles and summing up.
            var soldArticleService = new ArticleAcheteFileService();
            var path = "config/articlesAchetes.json";

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Aucun article vendu trouvé.");
                return;
            }

            var soldArticles = soldArticleService.ReadFromFile(path);

            float totalAmount = 0;
            foreach (var soldArticle in soldArticles)
            {
                // Assuming the price of the article remains same. Otherwise, we'd need to fetch the actual price from config/articles.json
                totalAmount += soldArticle.Quantite * (GetArticlePriceById(soldArticle.IdChocolat));
            }

            File.WriteAllText("TotalSalesBill.txt", $"Total des articles vendus: {totalAmount}€");
            Console.WriteLine("Facture créée: TotalSalesBill.txt");
            Console.Clear();
        }

        private void GenerateBillByBuyer()
        {
            Console.Clear();
            var soldArticleService = new ArticleAcheteFileService();
            var path = "config/articlesAchetes.json";

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
            Console.Clear();
        }

        private void GenerateBillByDate()
        {
            Console.Clear();
            var soldArticleService = new ArticleAcheteFileService();
            var path = "config/articlesAchetes.json";

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
            Console.Clear();
        }


        private float GetArticlePriceById(Guid id)
        {
            var articleService = new ArticleFileService();
            var path = "config/articles.json";
            var articles = articleService.ReadFromFile(path);
            var article = articles.Find(a => a.Id == id);

            return article?.Prix ?? 0;
        }

        public bool IsValidPassword(string password)
        {
            var regex = new Regex(@"^(?=.*[a-zA-Z0-9])(?=.*[^a-zA-Z0-9])(?=.*[A-Z]).{6,}$");
            return regex.IsMatch(password);
        }


        private void Logout()
        {
            Console.WriteLine("Déconnexion réussie!");
        }


        public void HandleUtilisateur()
        {
            var buyerService = new AcheteurFileService();
            var pathAcheteursJson = "config/acheteurs.json";

            Console.Write("Entrez votre nom: ");
            Console.Write("> ");
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

                var welcomeMessage = $"Bienvenue, {buyer.Prenom} {buyer.Nom}!";
                
                Console.WriteLine(new string('-', welcomeMessage.Length));
                Console.WriteLine(welcomeMessage);
                Console.WriteLine(new string('-', welcomeMessage.Length));
                
                Console.WriteLine();

                // Log the addition of an article
                ProjetChocolat.Logging.Logger.LogAction(buyer.Nom, "Connexion", "acheteur existant");
            }
            else
            {
                // Console.WriteLine("Création d'un nouvel acheteur.");
                var newBuyerMessage = "Création d'un nouvel acheteur.";
                Console.WriteLine(new string('-', newBuyerMessage.Length));
                Console.WriteLine(newBuyerMessage);
                Console.WriteLine(new string('-', newBuyerMessage.Length));
                buyer = new Acheteur(); // buyer is declared outside of the else block
                buyer.Id = Guid.NewGuid();
                

                buyer.Nom = enteredUsername;

                Console.Write("Entrez votre prénom: ");
                Console.Write("> ");
                buyer.Prenom = Console.ReadLine();
                

                Console.Write("Entrez votre adresse: ");
                Console.Write("> ");
                buyer.Adresse = Console.ReadLine();
                

                string phoneNumberInput;
                int phoneNumber;
                do
                {
                    Console.Write("Entrez votre téléphone: ");
                    Console.Write("> ");
                    phoneNumberInput = Console.ReadLine();
                } while (!int.TryParse(phoneNumberInput, out phoneNumber) || phoneNumber < 0);

                buyer.Telephone = phoneNumber;

                buyerService.WriteToFile(pathAcheteursJson, new List<Acheteur> { buyer });

                // Log the creation of the new buyer
                ProjetChocolat.Logging.Logger.LogAction(enteredUsername, "Création", "nouvel acheteur");

                Console.WriteLine($"Bienvenue, {buyer.Prenom} {buyer.Nom}!");
            }


            var articleService = new ArticleFileService();
            var pathArticlesJson = "config/articles.json";
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
                    $"{i + 1}: Reference: {article.Reference}, Prix: {article.Prix}");
            }


            var articleAcheteService = new ArticleAcheteFileService();
            var pathArticleAchetesJson = "config/articlesAchetes.json";

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
                        ProjetChocolat.Logging.Logger.LogAction(buyer.Nom, "Achat de", $"{articleToAdd.Reference}");

                        // Add to the purchased articles list
                        var existingArticleAchete = purchasedArticles.FirstOrDefault(a =>
                            a.IdAcheteur == buyer.Id && a.IdChocolat == articleToAdd.Id);
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