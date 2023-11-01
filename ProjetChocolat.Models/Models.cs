using System;

namespace ProjetChocolat.Models
{
    public class Administrateur
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; } // Dans une vraie application, ne jamais stocker les mots de passe en clair.
    }

    public class Acheteur
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Adresse { get; set; }
        public int Telephone { get; set; } // Peut-être qu'un string serait mieux pour gérer les formats de numéros de téléphone internationaux.
    }

    public class Article
    {
        public Guid Id { get; set; }
        public string Reference { get; set; }
        public float Prix { get; set; }
    }

    public class ArticleAchete
    {
        public Guid IdAcheteur { get; set; }
        public Guid IdChocolat { get; set; }
        public int Quantite { get; set; }
        public DateTime DateAchat { get; set; }
    }
}