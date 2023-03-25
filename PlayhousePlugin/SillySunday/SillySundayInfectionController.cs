using PlayerRoles;

namespace PlayhousePlugin
{
    public class SillySundayInfectionController
    {
        public static bool InfectionEnabled = false;
        public static RoleTypeId InfectedRole = RoleTypeId.None;
        
        public static void ResetToDefaults()
        {
            InfectionEnabled = false;
            InfectedRole = RoleTypeId.None;
        }
    }
}