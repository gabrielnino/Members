namespace LiveNetwork.Domain
{
    public sealed class ExperienceModel
    {
        public string Company { get; set; } = "";
        public string CompanyUrl { get; set; } = "";
        public string CompanyLogoUrl { get; set; } = "";
        public string CompanyLogoAlt { get; set; } = "";
        public string EmploymentSummary { get; set; } = "";   // e.g., "Full-time · 5 yrs"
        public string Location { get; set; } = "";            // e.g., "Herndon, Virginia, United States"
        public List<ExperienceRoleModel> Roles { get; set; } = new();
    }

    public sealed class ExperienceRoleModel
    {
        public string Title { get; set; } = "";               // e.g., "Technical Recruiter"
        public string DateRange { get; set; } = "";           // e.g., "Sep 2020 - Present · 5 yrs"
        public string WorkArrangement { get; set; } = "";     // e.g., "On-site", "Remote", "Hybrid"
        public string Description { get; set; } = "";         // flattened text/bullets
        public string ContextualSkills { get; set; } = "";    // e.g., "Information Technology"
    }

    public sealed class EducationModel
    {
        public string School { get; set; } = "";
        public string SchoolUrl { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public string LogoAlt { get; set; } = "";

        public string Degree { get; set; } = "";      // e.g., "Master of Business Administration - MBA"
        public string Field { get; set; } = "";       // e.g., "Human Resources Management and Services"
        public string DateRange { get; set; } = "";   // e.g., "2011 - 2014" or "Oct 2022"
        public string Description { get; set; } = ""; // optional
    }


    public class LinkedInProfile
    {
        public string FullName { get; set; }
        public string Headline { get; set; }
        public string Location { get; set; }
        public string CurrentCompany { get; set; }
        public string ProfileImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string ConnectionDegree { get; set; }
        public string Connections { get; set; }
        public string Followers { get; set; }
        public string AboutText { get; set; }
        public required Uri Url { get; set; }
        public List<ExperienceModel> Experiences { get; set; } = [];
        public List<EducationModel> Educations { get; set; } = [];

    }

}
