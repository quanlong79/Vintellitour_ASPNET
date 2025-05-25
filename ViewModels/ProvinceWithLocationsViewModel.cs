using System.Collections.Generic;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.ViewModels
{
    public class ProvinceWithLocationsViewModel
    {
        public Provinces Province { get; set; }
        public List<LocationsModel> Locations { get; set; }
    }
}
