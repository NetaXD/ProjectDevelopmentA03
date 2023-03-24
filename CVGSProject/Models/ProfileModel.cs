using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVGSProject.Models {
    public class ProfileModel {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int ReceiveNews { get; set; }
        public List<Platforms> Platforms { get; set; }
        public List<Categories> Categories { get; set; }
        public Address Address { get; set; }
        public List<CreditCard> CreditCards { get; set; }

    }

    public class Platforms {
        public string Platform { get; set; }
    }
    public class Categories {
        public string Category { get; set; }
    }
    public class Address {
        public string Mailing { get; set; }
        public string Shipping { get; set; }
        public bool SameAddress { get; set; }
    }
    public class CreditCard {
        public string CreditCardNumber { get; set; }
    }

    public class Games {
        public List<Game> GamesList { get; set; }
    }
    public class Game {
        public int GameId { get; set; }
        public string GameTitle { get; set; }

        public string GamePublisher { get; set; }

        public string GameDescription { get; set; }

        public string GameRating { get; set; }
    }
    public class WishList {
        public List<Wish> Wishes { get; set; }
    }
    public class Wish {
        public int WishListId { get; set; }
        public int AccountId { get; set; }
        public int GameID { get; set; }
        public string GameTitle { get; set; }
    }
    public class RatingHelper {
        public int GameId { get; set; }
        public int Ratings { get; set; }
        public int AmountOfRatings { get; set; }
    }
}