﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rbl.Helpers;
using Rbl.Models;
using Rbl.Models.Report;
using Rbl.Services;

namespace Rbl.Pages
{
    public class ReportModel : PageModel
    {
        private readonly IRblDataService _service;

        public ReportModel(IRblDataService service)
        {
            _service = service;
        }

        public Organization Organization { get; set; }
        public IDictionary<int, GeneralScoreResponse> YearlyScoresByTicker { get; set; } = new Dictionary<int, GeneralScoreResponse>();
        public GeneralScoreResponse ScoresByTicker { get; set; }
        public GeneralScoreResponse ScoresAll { get; set; }
        public GeneralScoreResponse ScoresIndustry { get; set; }
        public GeneralScoreResponse ScoresTop10 { get; set; }
        public GeneralScoreResponse LastInTopTenTotal { get; set; }

        public string CompanyName { get; set; }
        public string YearString {get;set;}

        public decimal OrganizationScoreTotal { get; set; }
        public decimal AllScoreTotal { get; set; }
        public decimal TopTenScoreTotal { get; set; }
        public decimal IndustryScoreTotal { get; set; }
        public string LeadershipEoReport5 { get; set; }
        public string OrganizationEoReport5 { get; set; }
        public string TalentEoReport5 { get; set; }
        public string HrEoReport5 { get; set; }
        public string Report6Narrative { get; set; }
        public string Report6FollowUp { get; set; }
        public string G3HCAI_TalentPageId = "https://www.g3humancapability.com";
        public string G3HCAI_LeadershipPageId = "https://www.g3humancapability.com";
        public string G3HCAI_OrganizationPageId = "https://www.g3humancapability.com";
        public string G3HCAI_HrPageId = "https://www.g3humancapability.com";


        public async Task<IActionResult> OnGetAsync(string ticker, int year = 2021)
        {
            if (String.IsNullOrEmpty(ticker))
            {
                return NotFound();
            }

            if(year == 0) {
                return NotFound("No year provided");
            }
            YearString = $"{year}";

            Organization = await _service.GetOrganizationByTicker(ticker);

            if (Organization == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(Organization.sec_name))
                CompanyName = Organization.sec_name;
            else
                return NotFound("Could not find the company's SEC name");

            ScoresByTicker = await _service.GetOrganizationScoresByTicker(year, ticker);
            for (int i = 0; i < 5; i++)
            {
                int curYear = year - i;
                if (!(await _service.OrganizationHasScoreForYear(curYear, ticker)))
                    break;

                YearlyScoresByTicker[curYear] = await _service.GetOrganizationScoresByTicker(curYear, ticker);
            }

            ScoresAll = await _service.GetScoresAll(year);
            ScoresIndustry = await _service.GetScoresByIndustry(year, Organization.industry_code);
            ScoresTop10 = await _service.GetScoresTopTen(year);
            LastInTopTenTotal = await _service.GetScoresTotalForLastInTopTen(year);


            IndustryScoreTotal = (decimal)(ScoresIndustry.HrScore + ScoresIndustry.LeadershipScore +
                                           ScoresIndustry.OrgScore + ScoresIndustry.TalentScore);

            OrganizationScoreTotal = (decimal)(ScoresByTicker.HrScore + ScoresByTicker.LeadershipScore +
                                                ScoresByTicker.OrgScore + ScoresByTicker.TalentScore);

            AllScoreTotal = (decimal)(ScoresAll.HrScore + ScoresAll.LeadershipScore +
                                       ScoresAll.OrgScore + ScoresAll.TalentScore);

            TopTenScoreTotal = (decimal)Math.Round(LastInTopTenTotal.TotalScore, 2);

            TalentEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Talent, ScoresByTicker.TalentScore);
            OrganizationEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Organization, ScoresByTicker.OrgScore);
            LeadershipEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Leadership, ScoresByTicker.LeadershipScore);
            HrEoReport5 = GetEndOfReport5Sentences(CompanyName, WordTypesEnum.Hr, ScoresByTicker.HrScore);
            (Report6Narrative, Report6FollowUp, _) = GetReport6Sentence(ScoresByTicker);

            return Page();
        }

        private string GetEndOfReport5Sentences(string companyName, WordTypesEnum type, double? score)
        {
            var result = "No Score Found";
            if (score.HasValue == true)
            {

                var range1 = score.Value > 0 && score.Value < 5;
                var range2 = score.Value >= 5 && score.Value < 8;
                var range3 = score.Value >= 8 && score.Value < 11;
                switch (type)
                {
                    case WordTypesEnum.Talent:
                        if (range1)
                            result = $"{companyName} scores low in talent. Businesses that score low in talent tend to underinvest in people programs and are reactive to talent challenges. You likely have difficulty attracting and retaining the talent you need to win. We recommend that you take a more aggressive approach to ensuring that your employees have both the skills and commitment to deliver your business results. In the G3HC Actionable Insights the Talent pathway, there are 10 talent initiatives (such as acquiring talent, developing employees, encouraging diversity, and creating a positive employee experience) that you can choose from that have high impact on business outcomes.";
                        if (range2)
                            result = $"{companyName} scores moderately in talent. You are a solid choice for  to work, but not distinctive.  Your talent initiatives are adequate, but not best in class.  We recommend that you choose one or two high impact talent initiatives in the G3HC Actionable Insights where you can become best in class. When you excel in targeted talent initiatives, your people will be more able to deliver results that matter to your company.";
                        if (range3)
                            result = $"Congratulations! {companyName} scores high in the talent pathway. {companyName} is a talent magnet and the quality of your talent helps you achieve your business outcomes. Businesses that score high in talent tend to have competent employees (through acquiring talent, managing employee performance, and developing employees) who are committed (by tracking employee engagement and having a positive employee experience). We recommend you continue to invest in innovative talent initiatives to continue to have talent strengths.";
                        break;

                    case WordTypesEnum.Organization:
                        if (range1)
                            result = $"{companyName} scores low in organization capabilities. Often when you invest in in Human Capability, you focus more on developing individual competence than on building capabilities.  While individual talent matters, organizations and teamwork matter even more. Targeting and building organization capabilities has not been a priority for your business Your business lacks a strong, positive reputation for any organization capability which negatively impacts customer, financial and community results. We recommend that you look at the 12 organization capabilities in the G3HC Actionable Insights and pick one or two that you think will be critical for your success.  ";
                        if (range2)
                            result = $"{companyName} has a moderate score in organization capabilities. You have created organization capabilities but they may not yet be distinctive in the eyes of your customers and investors. . Capabilities with the highest impact give customers and investors confidence that your organization will meet their needs.  We recommend that you target an organization capability for improvement consistent with your business strategy.  In the G3HC Actionable Insights  we propose 12 organization capabilities you might chose to excel at. Establishing strategic clarity and acting with agility has high impact on business outcomes but so do the other capabilities, when done at a best-in-class level.";
                        if (range3)
                            result = $"Congratulations! {companyName} has industry best or world class organization capabilities. You have a distinctive reputation for how you operate in the eyes of your customers, employees and investors.  There is high confidence that you will deliver the right results the right way. Because of your reputation for capabilities, you have an advantage in attracting top employees and your customers tend to get the right experience when they do business with you. In the G3HC Actionable Insights the organization pathway suggests 12 capabilities and we encourage you to stay best in class in the capabilities that matter most to you.  ";
                        break;

                    case WordTypesEnum.Leadership:
                        if (range1)
                            result = $"{companyName} leadership score is low. When your business invests in leaders, you often focus more on developing individual leaders than on building a sustainable leadership capability. Investments in individual leaders are often hit or miss. Building leadership capability - investing in the next generation of leaders as well as leaders at all levels has not been a priority. As a result, your business lacks a strong, positive reputation for leadership which affects financial and customer results. In the G3HC Actionable Insights the leadership pathway has 6 initiatives you can choose from to improve your leadership score. Clarifying a business case for leadership and ensuring leadership reputation tend to have the highest impact on results.";
                        if (range2)
                            result = $"{companyName} has a moderate leadership score. Your leaders are generally as good as others in your industry but not distinctive. You have likely worked to improve leadership by defining what leaders should know and do, assessing leaders, and developing leaders, but your leadership initiatives have not made you a top company for leadership.  Your leaders may be good, but not great; generic, but not branded. We encourage you look at the six steps to improving leadership and to pick one or two of these six steps where you can excel.  In particular, clarifying a business case and ensuring reputation tend to have impact on results.";
                        if (range3)
                            result = $"Congratulations! {companyName} has very strong leadership. You have leaders at all levels who set the vision, execute strategies, manage people, upgrade future talent, and take care of themselves.  As a result, your customers and investors have high confidence that your leaders will deliver the right results the right way. Your organization has a reputation for leadership capability that is respected by employees, customers, investors, and communities. In the G3HC Actionable Insights the leadership pathway has 6 initiatives you can choose from to build even stronger leadership capability. We encourage you to keep investing in leadership, particularly clarifying the business case and ensuring reputation that has very high impact.";
                        break;

                    case WordTypesEnum.Hr:
                        if (range1)
                            result = $"{companyName} scores low in the HR pathway. HR departments that score in this range tend to focus HR on the administrative tasks of managing people, such as payroll, benefits, and compliance.  HR departments focus on implementing standard HR practices around people (hiring, training), performance (appraisal and rewards), information (communication), and work (policies and digital HR). HR is perceived as an administrative expense with a goal of reducing HR costs. In the G3HC Actionable Insights,the HR pathway includes 10 characteristics of effective HR where you could improve. We recommend that you start with improving HR Reputation - what you want HR to be known for at {companyName}.";
                        if (range2)
                            result = $"{companyName} has a moderate score for the HR pathway meaning that HR tends to be efficient at the essential elements of managing how people-related work is done.  In addition, HR is functionally excellent by offering accepted HR practices and responding to the needs of the business.   HR professionals are credible because they execute projects that support internal business stakeholders (line managers and employees) through a variety of HR practices. In the G3HC Actionable Insights, you will find a HR pathway that includes 10 initiatives. You should pick one or two of these initiatives where you want to excel in HR.";
                        if (range3)
                            result = $"Congratulations! {companyName} scores high in HR. HR does the essential work of HR efficiently and offers innovative HR practices.  In addition, HR also has a strong and positive reputation for creating value for the business. HR offers Human Capability (talent, leadership, organization) solutions that create value for all stakeholders.  HR professionals have positive relationships with each other and with line managers as the work together to meet business needs.  HR work also aligns with customers and investors and helps to deliver the strategic intent of the business.  We recommend you continue to innovate HR work listed in the G3HC Actionable Insights to meet the needs of all stakeholders.";
                        break;

                    default:
                        result = "Invalid type request";
                        break;

                }
            }

            return result;
        }

        public (string, string, string) GetReport6Sentence(GeneralScoreResponse scores)
        {
            var anyBelow5 = scores.TalentScore < 5 || scores.OrgScore < 5 || scores.LeadershipScore < 5 || scores.HrScore < 5;
            if (anyBelow5)
            {
                var below5 = new List<string>();
                if (scores.TalentScore < 5)
                    below5.Add($"<a href='{G3HCAI_TalentPageId}' class='red'>Section 1 (Talent)</a>");
                if (scores.LeadershipScore < 5)
                    below5.Add($"<a href='{G3HCAI_LeadershipPageId}' class='red'>Section 2 (Leadership)</a>");
                if (scores.OrgScore < 5)
                    below5.Add($"<a href='{G3HCAI_OrganizationPageId}' class='red'>Section 3 (Organization)</a>");
                if (scores.HrScore < 5)
                    below5.Add($"<a href='{G3HCAI_HrPageId}' class='red'>Section 4 (Human Resources)</a>");

                var appendixStr = string.Join(", ", below5);
                var followup = $"Based on your results; we recommend you take some time to review these links to G3HC Actionable Insights {appendixStr}.";
                return (
                    "<b>Improve on weaknesses</b>. Your scores show pathways below parity. Any pathway <b>below</b> parity should be a priority for improvement because pathways are interdependent.  Poor performance in any pathway drags down the others. These pathways should be targeted for improvement first ",
                    followup,
                    "Improve on weaknesses"
                    );
            }
            else
            {
                var numGreaterThan8 = (scores.TalentScore >= 8 ? 1 : 0) +
                    (scores.OrgScore >= 8 ? 1 : 0) +
                    (scores.LeadershipScore >= 8 ? 1 : 0) +
                    (scores.HrScore >= 8 ? 1 : 0);
                var numGreaterThan7 = (scores.TalentScore >= 7 ? 1 : 0) +
                    (scores.OrgScore >= 7 ? 1 : 0) +
                    (scores.LeadershipScore >= 7 ? 1 : 0) +
                    (scores.HrScore >= 7 ? 1 : 0);

                // Rule 3
                if (numGreaterThan8 >= 1 && numGreaterThan7 >= 2)
                {
                    return (
                        "<b>Prioritize strengths</b>. Your business is already superior. You score at industry best/ <b>world class</b> in one or more pathways. Your focus should be on prioritizing your strengths. You can shift away from comparisons to others and work on “conceptual best”. Conceptual best is imagining ways to ensure your reputation for distinctiveness in this area. In very strong businesses we see intention away from optimizing an individual pathway to integrating across pathways to increase Human Capability and stakeholder value.  These businesses have a strong, positive reputation and are very attractive to employees, customers, investors, and communities",
                        "Based on your results; we recommend you review our G3HC Actionable Insights.",
                        ""
                        );
                }
                // Rule 2
                else
                {
                    return (
                        "<b>Build on strengths</b>.  You do not have any scores below parity. Your scores do not show any scores at <b>industry best or world class</b> level. Your business should strive to be industry best or world class in one pathway. When this occurs, your company has a distinctive reputation which attracts employees, customers, investors, and communities. Based on your business strategy, identify which pathway should be at industry best and then invest there. ",
                        "Based on your results; we recommend you review our G3HC Actionable Insights.",
                        ""
                        );
                }
            }
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