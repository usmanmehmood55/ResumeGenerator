using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;
using System.Text.RegularExpressions;

namespace ResumeGenerator
{
    internal class Program
    {
        #region LAYOUT_CUSTOMIZATIONS

        // Font sizes
        const int NameFontSize       = 12;
        const int HeadingFontSize    = 10;
        const int SubHeadingFontSize = 9;
        const int TextFontSize       = 9;

        // Colors
        static readonly string HeadingColor    = Colors.Black;
        static readonly string SubHeadingColor = Colors.Black;
        static readonly string TextColor       = Colors.Black;

        // Font Family
        const string FontFamily = "Arial";

        // Spacing and Padding
        const float SectionSpacing      = 5;
        const float ElementSpacing      = 5;
        const float CellPadding         = 2;
        const float BulletPointIndent   = 5;
        const float BulletSymbolWidth   = 10;

        // Styles
        static TextStyle DefaultTextStyle => TextStyle.Default.FontFamily(FontFamily).FontSize(TextFontSize).FontColor(TextColor);
        static TextStyle HeadingStyle => TextStyle.Default.FontFamily(FontFamily).FontSize(HeadingFontSize).FontColor(HeadingColor).Bold();
        static TextStyle SubHeadingStyle => TextStyle.Default.FontFamily(FontFamily).FontSize(SubHeadingFontSize).FontColor(SubHeadingColor).Bold();
        static IContainer CellStyle(IContainer container) => container.Padding(CellPadding);

        #endregion

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please add only the input file path as an arg.");
                return -1;
            }

            string inputFilePath = args[0];
            if (Path.Exists(inputFilePath) == false)
            {
                Console.WriteLine("Input file does not exist.");
                return -2;
            }

            QuestPDF.Settings.License = LicenseType.Community;

            Root root = ResumeJson.Parse(inputFilePath);

            string docName = (root.About != null && root.About.Name != null) ?
                $"{root.About.Name.Replace(' ', '_')}.pdf" : "generated_resume.pdf";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(DefaultTextStyle);

                    // Content
                    page.Content().Column(column =>
                    {
                        column.Spacing(SectionSpacing);

                        // Header Section
                        column.Item().Element(c => HeaderSection(c, root));

                        // About Me Section
                        column.Item().Element(c => AboutSection(c, root));

                        // Experience Section
                        column.Item().Element(c => ExperienceSection(c, root));

                        // Skills Section
                        column.Item().Element(c => EducationSection(c, root));
                    });
                });
            })
                .GeneratePdf(docName);
                // .ShowInCompanion();

            Console.WriteLine("Resume generated successfully.");

            return 0;
        }

        // Header Section
        static void HeaderSection(IContainer container, Root root)
        {
            container.Column(column =>
            {
                if (root.About != null && root.About.Name != null)
                {
                    column.Item()
                        .AlignCenter()
                        .Text(root.About.Name.ToUpper())
                        .Style(SubHeadingStyle)
                        .FontSize(NameFontSize);
                }

                if (root.Links != null)
                {
                    column.Item()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Black)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // row 1 col 1
                        if (!string.IsNullOrEmpty(root.Links.Email))
                        {
                            table.Cell().Element(CellStyle).Text(root.Links.Email)
                                .Style(DefaultTextStyle);
                        }
                        else
                        {
                            Console.WriteLine("MISSING: Email");
                            table.Cell();
                        }

                        // row 1 col 2
                        if (!string.IsNullOrEmpty(root.Links.PhoneNumber))
                        {
                            table.Cell().AlignRight().Element(CellStyle).Text(root.Links.PhoneNumber)
                                .Style(DefaultTextStyle);
                        }
                        else
                        {
                            Console.WriteLine("MISSING: Phone");
                            table.Cell();
                        }

                        // row 2 col 1
                        if (!string.IsNullOrEmpty(root.Links.PortfolioWebsite))
                        {
                            var elem = table.Cell().AlignLeft().Element(CellStyle);
                            elem = elem.Hyperlink(root.Links.PortfolioWebsite);
                            elem.Text(CleanUrl(root.Links.PortfolioWebsite))
                                .Style(DefaultTextStyle);
                        }
                        else
                        {
                            Console.WriteLine("MISSING: PortfolioWebsite");
                            table.Cell();
                        }

                        // row 2 col 2
                        if (!string.IsNullOrEmpty(root.Links.Linkedin))
                        {
                            var elem = table.Cell().AlignRight().Element(CellStyle);
                            elem = elem.Hyperlink(root.Links.Linkedin);
                            elem.Text(CleanUrl(root.Links.Linkedin))
                                .Style(DefaultTextStyle);
                        }
                        else
                        {
                            Console.WriteLine("MISSING: Linkedin");
                            table.Cell();
                        }
                    });
                }
            });
        }

        private static string CleanUrl(string url)
        {
            Regex regex = new(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.None);
            Match match = regex.Match(url);
            if (match.Success && !string.IsNullOrEmpty(match.Value)) url = match.Value.TrimEnd('/');
            else Console.WriteLine($"{url} is strange and I refuse to process it.");
            return url;
        }

        // About Me Section
        static void AboutSection(IContainer container, Root root)
        {
            if (root.About is null)
                return;

            container.Column(column =>
            {
                if (root.Experiences != null && root.Experiences.Count > 0)
                {
                    column.Item()
                        .PaddingTop(5)
                        .AlignCenter()
                        .Text(root.Experiences[0].Position)
                        .Style(HeadingStyle);
                }

                if (root.About.ProfessionalSummaryBullets != null)
                    BulletsSection("Professional Summary", root.About.ProfessionalSummaryBullets, column);
                else Console.WriteLine("Professional Summary missing");

                if (root.About.TechnicalSkills != null)
                    BulletsSection("Technical Skills", root.About.TechnicalSkills, column);
                else Console.WriteLine("Technical Skills missing");
            });
        }

        private static void BulletsSection(string sectionName, List<string> autobiographyBullets, ColumnDescriptor column)
        {
            column.Spacing(ElementSpacing);
            column.Item()
                .BorderBottom(0.5f)
                .BorderColor(Colors.Black)
                .Text(sectionName)
                .Style(HeadingStyle);

            column.Item().PaddingLeft(BulletPointIndent).Column(bulletColumn =>
            {
                bulletColumn.Spacing(1); // Adjust spacing between bullet points

                foreach (var point in autobiographyBullets)
                {
                    bulletColumn.Item().Row(row =>
                    {
                        // Bullet symbol
                        row.ConstantItem(BulletSymbolWidth).Text("•")
                            .FontSize(TextFontSize);

                        // Bullet point text
                        row.RelativeItem().Text(point)
                            .FontSize(TextFontSize)
                            .LineHeight(1.3f);
                    });
                }
            });
        }

        // Experience Section
        static void ExperienceSection(IContainer container, Root root)
        {
            if (root.Experiences is null)
                return;

            container.Column(column =>
            {
                column.Spacing(ElementSpacing + 4);
                column.Item()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Black)
                    .Text("Experience")
                    .Style(HeadingStyle);

                foreach (var exp in root.Experiences)
                {
                    column.Item().Element(c => ExperienceItem(c, exp));
                }
            });
        }

        static void ExperienceItem(IContainer container, Experience exp)
        {
            if (exp.Company is null)
                return;

            container.PaddingBottom(SectionSpacing).Column(column =>
            {
                column.Spacing(4);

                // Position and Company
                column.Item()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Black)
                    .PaddingBottom(2)
                    .Row(row =>
                    {
                        var ExpNameContainer = row.RelativeItem().AlignLeft();

                        if (!string.IsNullOrEmpty(exp.Company.Link))
                        {
                            ExpNameContainer = ExpNameContainer.Hyperlink(exp.Company.Link);
                        }

                        ExpNameContainer.Text($"{exp.Position} at {exp.Company.Name}")
                            .Style(SubHeadingStyle);

                        row.ConstantItem(100).AlignRight().Text(exp.Duration)
                            .FontSize(TextFontSize)
                            .Italic();
                    });

                // Bullet Points
                if (exp.BulletPoints != null && exp.BulletPoints.Count > 0)
                {
                    column.Item().PaddingLeft(BulletPointIndent).Column(bulletColumn =>
                    {
                        bulletColumn.Spacing(2); // Adjust spacing between bullet points

                        foreach (var point in exp.BulletPoints)
                        {
                            bulletColumn.Item().Row(row =>
                            {
                                // Bullet symbol
                                row.ConstantItem(BulletSymbolWidth).Text("•")
                                    .FontSize(TextFontSize);

                                // Bullet point text
                                row.RelativeItem().Text(point)
                                    .FontSize(TextFontSize)
                                    .LineHeight(1.3f);
                            });
                        }
                    });
                }
            });
        }

        // Education Section
        static void EducationSection(IContainer container, Root root)
        {
            if (root.Educations is null)
            {
                Console.WriteLine("Education MISSING!");
                return;
            }
            
            container.Column(column =>
            {
                column.Spacing(ElementSpacing);
                column.Item()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Black)
                    .Text("Education")
                    .Style(HeadingStyle);

                foreach(Education education in root.Educations)
                {
                    column.Item()
                        .BorderBottom(0.5f)
                        .BorderColor(Colors.Black)
                        .PaddingBottom(2)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"{education.Degree} in {education.Major}")
                                .Style(SubHeadingStyle);

                            row.ConstantItem(100).AlignRight().Text(education.Duration)
                                .FontSize(TextFontSize)
                                .Italic();
                        });

                    if (education.Institute != null)
                    {
                        column.Item().Text(education.Institute.Name)
                            .FontSize(TextFontSize)
                            .LineHeight(1.5f);
                    }
                }
            });
        }
    }
}
