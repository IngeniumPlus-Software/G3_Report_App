# G3_Report_App

# Development Environment Setup
- Copy appsettings.json into Rbl project folder
  - https://ingeniumplus.sharepoint.com/:u:/s/IPSDevelopmentTeam/EarXRFhOTedOmBv0PUhxkAABVK9KW7SGNWgVTlguNjvl1w?e=YlvRH8

# API Calls
- [POST] /api/reports/{code}
  - Checks if the provided code is valid
- [GET] /api/reports/{code}
  - Returns the FileContentResult PDF for the provided code
- [POST] /api/reports/{secretKey}/reset/{secretValue}
  - Clears all the cached PDFs