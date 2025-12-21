namespace RSD_E_Learning.Services
{
    public static class EmailTemplates
    {
        public static string StudentDeactivated(string name)
        {
            return $@"
                <p>Dear {name},</p>
                <p>Your student account has been <strong>deactivated</strong> by the administrator.</p>
                <p>If you believe this is a mistake, please contact support.</p>
                <br />
                <p>RSD E-Learning Team</p>";
        }

        public static string StudentActivated(string name)
        {
            return $@"
                <p>Dear {name},</p>
                <p>Your student account has been <strong>re-activated</strong>.</p>
                <p>You may now log in again.</p>
                <br />
                <p>RSD E-Learning Team</p>";
        }
    }
}
