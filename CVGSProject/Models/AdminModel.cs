using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace CVGSProject.Models {
    public class AdminModel {
        public class AdminPanelModel {
            public List<GamesModel> GamesList { get; set; }
        }
        public class GamesModel {
            public int GamesID { get; set; }
            public string GameTitle { get; set; }
            public string GamesPublisher { get; set; }
        }
    }
}