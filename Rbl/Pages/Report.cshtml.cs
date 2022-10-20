using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rbl.Helpers;
using Rbl.Models;
using Rbl.Services;

namespace Rbl.Pages
{
    public class ReportModel : PageModel
    {
        private readonly Rbl.Models.RBLContext _context;
        private readonly IRblDataService _service;

        public ReportModel(Rbl.Models.RBLContext context, IRblDataService service)
        {
            _context = context;
            _service = service;
        }

        public Organization Organization { get; set; }
        public ScoresByTicker ScoresByTicker { get; set; }
        public ScoresAll ScoresAll { get; set; }
        public ScoresByIndustry ScoresIndustry { get; set; }
        public ScoresTopTen ScoresTop10 { get; set; }
        public ScoresTotal LastInTopTenTotal { get; set; }

        public string CompanyName { get; set; }

        public decimal OrganizationScoreTotal { get; set; }
        public decimal AllScoreTotal { get; set; }
        public decimal TopTenScoreTotal { get; set; }
        public decimal IndustryScoreTotal { get; set; }
        public string[] TalentSentences { get; set; } = new string[0];
        public string[] LeadershipSentences { get; set; } = new string[0];
        public string[] OrganizationSentences { get; set; } = new string[0];
        public string[] HrSentences { get; set; } = new string[0];
        public string LeadershipEoReport5 { get; set; }
        public string OrganizationEoReport5 { get; set; }
        public string TalentEoReport5 { get; set; }
        public string HrEoReport5 { get; set; }
        public string Report6Narrative { get; set; }
        public string Report6FollowUp { get; set; }


        public async Task<IActionResult> OnGetAsync(string ticker)
        {
            if (String.IsNullOrEmpty(ticker))
            {
                return NotFound();
            }

            Organization = await _service.GetOrganizationByTicker(ticker);

            if (Organization == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(Organization.sec_name))
                CompanyName = Organization.sec_name;
            else
                return NotFound("Could not find the company's SEC name");

            var neededSentences = new List<WordTypesEnum> { WordTypesEnum.Talent, WordTypesEnum.Leadership, WordTypesEnum.Organization, WordTypesEnum.Hr };
            var cachedSentences = await _service.GetCachedSentences(ticker);
            if(cachedSentences == null)
                cachedSentences = new DfSentence { Ticker = ticker };
            else
            {
                if (!string.IsNullOrEmpty(cachedSentences.TalentSentences))
                    neededSentences.Remove(WordTypesEnum.Talent);

                if (!string.IsNullOrEmpty(cachedSentences.LeadershipSentences))
                    neededSentences.Remove(WordTypesEnum.Leadership);

                if (!string.IsNullOrEmpty(cachedSentences.OrganizationSentences))
                    neededSentences.Remove(WordTypesEnum.Organization);

                if (!string.IsNullOrEmpty(cachedSentences.HrSentences))
                    neededSentences.Remove(WordTypesEnum.Hr);
            }

            var importantWords = await _service.GetImportantWords(neededSentences.ToArray());
            var allSentences = await _service.GetSentenceResponse(ticker, importantWords);
            var anyUpdates = false;
            foreach(var t in neededSentences)
            {
                var tSentences = allSentences.GetRawHtml(t);  // LIVE
                //var tSentences = allSentences.GetRawHtml(ticker, t);    // DEBUG
                switch (t)
                {
                    case WordTypesEnum.Talent:
                        cachedSentences.TalentSentences = string.Join("\\n\\n", tSentences);
                        anyUpdates = true;
                        break;
                    case WordTypesEnum.Leadership:
                        cachedSentences.LeadershipSentences = string.Join("\\n\\n", tSentences);
                        anyUpdates = true;
                        break;
                    case WordTypesEnum.Organization:
                        cachedSentences.OrganizationSentences = string.Join("\\n\\n", tSentences);
                        anyUpdates = true;
                        break;
                    case WordTypesEnum.Hr:
                        cachedSentences.HrSentences = string.Join("\\n\\n", tSentences);
                        anyUpdates = true;
                        break;
                }
            }

            if(anyUpdates)
                await _service.SaveCachedSentences(cachedSentences);

            TalentSentences = cachedSentences.TalentSentences.Split("\\n\\n");
            LeadershipSentences = cachedSentences.LeadershipSentences.Split("\\n\\n");
            OrganizationSentences = cachedSentences.OrganizationSentences.Split("\\n\\n");
            HrSentences = cachedSentences.HrSentences.Split("\\n\\n");

            ScoresByTicker = await _service.GetOrganizationScoresByTicker(ticker);
            ScoresAll = await _service.GetScoresAll();
            ScoresIndustry = await _service.GetScoresByIndustry(Organization.industry_code);
            ScoresTop10 = await _service.GetScoresTopTen();
            LastInTopTenTotal = await _service.GetScoresTotalForLastInTopTen();

            IndustryScoreTotal = (decimal) (ScoresIndustry.HrScore + ScoresIndustry.LeadershipScore +
                                           ScoresIndustry.OrganizationScore + ScoresIndustry.TalentScore);

            OrganizationScoreTotal = (decimal) (ScoresByTicker.HrScore + ScoresByTicker.LeadershipScore +
                                                ScoresByTicker.OrganizationScore + ScoresByTicker.TalentScore);

            AllScoreTotal = (decimal) (ScoresAll.HrScore + ScoresAll.LeadershipScore +
                                       ScoresAll.OrganizationScore + ScoresAll.TalentScore);

            TopTenScoreTotal = (decimal) Math.Round((LastInTopTenTotal.TotalScore ?? 0), 2);

            TalentEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Talent, ScoresByTicker.TalentScore);
            OrganizationEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Organization, ScoresByTicker.OrganizationScore);
            LeadershipEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Leadership, ScoresByTicker.LeadershipScore);
            HrEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Hr, ScoresByTicker.HrScore);
            (Report6Narrative, Report6FollowUp, _) = GetReport6Sentence(ScoresByTicker);

            return Page();
        }

        private string GetEndOfReport5Sentences(string companyName, WordTypesEnum type, double? score)
        {
            var result = "No Score Found";
            if (score.HasValue == true) {

                var range1 = score.Value > 0 && score.Value < 5;
                var range2 = score.Value >= 5 && score.Value < 8;
                var range3 = score.Value >= 8 && score.Value < 11;
                switch (type)
                {
                    case WordTypesEnum.Talent:
                        if (range1)
                            result = $"{companyName} scores low in talent. Businesses that score low in talent tend to underinvest in people programs and are reactive to talent challenges. You likely have difficulty attracting and retaining the talent you need to win. We recommend that you take a more aggressive approach to ensuring that your employees have both the skills and commitment to deliver your business results. In the G3HC Actionable Insights, there are 10 talent initiatives (such as acquiring talent, developing employees, encouraging diversity, and creating a positive employee experience) that you can choose from that have high impact on business outcomes.";
                        if (range2)
                            result = $"{companyName} scores moderately in talent. You are a solid choice, but not distinctive, for employees to work.  Your talent initiatives are adequate, but not best in class.  We recommend that you choose one or two high impact talent initiatives in the G3HC Actionable Insights where you can become best in class. When you excel in targeted talent initiatives, your people will be more able to deliver results that matter to your company.";
                        if (range3)
                            result = $"Congratulations! {companyName} scores high in the talent pathway. {companyName} is a talent magnet and the quality of your talent helps you achieve your business outcomes. Businesses that score high in talent tend to have competent employees (through acquiring talent, managing employee performance, and developing employees) who are committed (by tracking employee engagement and having a positive employee experience). We recommend you continue to invest in innovative talent initiatives to continue to have talent strengths.";
                        break;

                    case WordTypesEnum.Organization:
                        if (range1)
                            result = $"{companyName} scores low in organization capabilities. Often when you invest in in Human Capability, you focus more on developing individual competence than on building capabilities.  While individual talent matters, organizations and teamwork matter even more. Targeting and building organization capabilities has not been a priority for your business Your business lacks a strong, positive reputation for any organization capability which negatively impacts customer, financial and community results. We recommend that you look at the 12 organization capabilities in the G3HC Actionable Insights and pick one or two that you think will be critical for your success.  ";
                        if (range2)
                            result = $"{companyName} has a moderate score in organization capabilities. You have created organization capabilities but they may not yet be distinctive in the eyes of your customers and investors. . Capabilities with the highest impact give customers and investor confidence that your organization will meet their needs.  We recommend that you target an organization capability for improvement consistent with your business strategy.  In the G3HC Actionable Insights  we propose 12 organization capabilities you might chose to excel at. Establishing strategic clarity and acting with agility has high impact on business outcomes but so do the other capabilities, when done at a best-in-class level.";
                        if (range3)
                            result = $"{companyName} has industry best or world class organization capabilities. You have a distinctive reputation for how you operate in eyes of your customers, employees and investors.  There is high confidence that you will deliver the right results the right way. Because of your reputation for capabilities, you have advantage in attracting top employees and your customers tend to get the right experience when they do business with you. In the G3HC Actionable Insights the framework for organization capabilities suggest 12 capabilities and we encourage you to stay best in class in the capabilities that matter most to you.  ";
                        break;

                    case WordTypesEnum.Leadership:
                        if (range1)
                            result = $"{companyName} leadership score is low. When your business invests in leaders, you often focus more on developing individual leaders than on building a sustainable leadership capability. Investments in individual leaders are often hit or miss. Building leadership capability- investing in the next generation of leaders as well as leaders at all levels has not been a priority. As a result, your business lacks a strong, positive reputation for leadership which affects financial and customer results. In the G3HC Actionable Insights the framework for leadership has 6 initiatives you can choose from to improve your leadership score. Clarifying a business case for leadership and ensuring leadership reputation tend to have the highest impact on results.";
                        if (range2)
                            result = $"{companyName} has a moderate leadership score. Your leaders are generally as good as others in your industry but not distinctive. You have likely worked to improve leadership by defining what leaders should know and do, assessing leaders, and developing leaders, but your leadership initiatives have not made you a top company for leadership.  Your leaders may be good, but not great; generic, but not branded. We encourage you look at the six steps to improving leadership and to pick one or two of these six steps where you can excel.  In particular, clarifying a business case and ensuring reputation tend to have impact on results.";
                        if (range3)
                            result = $"{companyName} has very strong leadership. You have leaders at all levels who set the vision, execute strategies, manage people, upgrade future talent, and take care of themselves.  As a result, your customers and investors have high confidence that your leaders will deliver the right results the right way. Your organization has a reputation for leadership capability that is respected by employees, customers, investors, and communities. In the G3HC Actionable Insights the framework for leadership has 6 initiatives you can choose from to build even stronger leadership capability. We encourage you to keep investing in leadership, particularly clarifying the business case and ensuring reputation that has  very high impact.";
                        break;

                    case WordTypesEnum.Hr:
                        if (range1)
                            result = $"{companyName} scores low in the HR pathway. HR departments that score in this range tend to focus HR on the administrative tasks of managing people, such as payroll, benefits, and compliance.  HR departments focus on implementing standard HR practices around people (hiring, training), performance (appraisal and rewards), information (communication), and work (policies and digital HR). HR is perceived as an administrative expense with a goal of reducing HR costs. In the G3HC Actionable Insights, you will find a framework for HR that includes 9 characteristics of effective HR where you could improve. We recommend that you start with improving HR Reputation- what you want HR to be known for at {companyName}.";
                        if (range2)
                            result = $"{companyName} has a moderate score for the HR pathway meaning that HR tends to be efficient at the essential elements of managing how people-related work is done.  In addition, HR is functionally excellent by offering accepted HR practices and responding to the needs of the business.   HR professionals are credible because they execute projects that support internal business stakeholders (line managers and employees) through a variety of HR practices. In the G3HC Actionable Insights, you will find a framework for HR that includes 9 initiatives. You should pick one or two of these initiatives where you want to excel in HR.";
                        if (range3)
                            result = $"{companyName} scores high in HR. HR does the essential work of HR efficiently and offers innovative HR practices.  In addition, HR also has a strong and positive reputation for creating value for the business. HR offers Human Capability (talent, leadership, organization) solutions that create value for all stakeholders.  HR professionals have positive relationships with each other and with line managers as the work together to meet business needs.  HR work also aligns with customers and investors and helps to deliver the strategic intent of the business.  We recommend you continue to innovate HR work listed in the G3HC Actionable Insights to meet the needs of all stakeholders.";
                        break;

                    default:
                        result = "Invalid type request";
                        break;

                }
            }

            return result;
        }

        public (string, string, string) GetReport6Sentence(ScoresByTicker scores)
        {
            var anyBelow5 = scores.TalentScore < 5 || scores.OrganizationScore < 5 || scores.LeadershipScore < 5 || scores.HrScore < 5;
            if (anyBelow5)
            {
                var below5 = new List<string>();
                if (scores.TalentScore < 5)
                    below5.Add("<a href='#page-31' class='red'>G3HC Actionable Insights 5 (Talent)</a>");
                if (scores.OrganizationScore < 5)
                    below5.Add("<a href='#page-35' class='red'>G3HC Actionable Insights 6 (Organization)</a>");
                if (scores.LeadershipScore < 5)
                    below5.Add("<a href='#page-40' class='red'>G3HC Actionable Insights 7 (Leadership)</a>");
                if (scores.HrScore < 5)
                    below5.Add("<a href='#page-43' class='red'>G3HC Actionable Insights 8 (HR)</a>");

                var appendixStr = string.Join(", ", below5);
                var followup = $"Based on your results; we recommend you take some time to review {appendixStr}.";
                return (
                    "<b>Improve on weaknesses</b>. Your scores show pathways below parity. Any pathway <b>below</b> parity should be priority for improvement because pathways are interdependent.  Poor performance in any pathway drags down the others. These pathways should be targeted for improvement first ",
                    followup,
                    "Improve on weaknesses"
                    );
            } else
            {
                var numGreaterThan8 = (scores.TalentScore >= 8 ? 1 : 0) +
                    (scores.OrganizationScore >= 8 ? 1 : 0) +
                    (scores.LeadershipScore >= 8 ? 1 : 0) +
                    (scores.HrScore >= 8 ? 1 : 0);
                var numGreaterThan7 = (scores.TalentScore >= 7 ? 1 : 0) +
                    (scores.OrganizationScore >= 7 ? 1 : 0) +
                    (scores.LeadershipScore >= 7 ? 1 : 0) +
                    (scores.HrScore >= 7 ? 1 : 0);

                // Rule 3
                if(numGreaterThan8 >= 1 && numGreaterThan7 >= 2)
                {
                    return (
                        "<b>Prioritize strengths</b>. Your business is already superior. You score at industry best/ <b>world class</b> in one or more pathways. Your focus should be on prioritizing your strengths. You can shift away from comparisons to others and work on “conceptual best”. Conceptual best is imagining ways to ensure your reputation for distinctiveness in this area. In very strong businesses we see intention away from optimizing an individual pathway to integrating across pathways to increase Human Capability and culture.  These businesses have a strong, positive reputation and are very attractive to employees, customers, investors, and communities",
                        "Based on your results; we recommend you review our G3HC Actionable Insights.",
                        ""
                        );
                } 
                // Rule 2
                else
                {
                    return (
                        "<b>Build on strengths</b>.  You do not have any scores below parity. Your scores do not show any scores at <b>industry best or world class</b> level. Your business should strive to be industry best or world class in one pathway. When this occurs, your company has a distinctive reputation which attracts customers, employees, customers, investors, and communities. Based on your business strategy, identify which pathway should be at industry best and then invest there. ",
                        "Based on your results; we recommend you review our G3HC Actionable Insights.",
                        ""
                        );
                }
            }

            // TODO: REMOVE
            return ("", "", "");
        }

        public string GetParityClassNames(double? score, bool myCompany = false)
        {
            var result = string.Empty;
            if (score < 5)
                result = "below-parity";
            if (score >= 8)
                result = "high-parity";

            if (!string.IsNullOrEmpty(result) && myCompany)
                result += " bold";

            return result;
        }
    }
}