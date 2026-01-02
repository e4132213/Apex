Project - Apex 

The task is to complete the development of an existing program. 
(dont mess with the Venues folder)

This program is supposed to be an intranet system using RAZOR PAGES programmed through visual studio.
A list of requirements have been provided to explain what needs to be added into the existing program.

A checklist in provided on OneDrive to keep track of what requirements have and havent been added yet.
(https://liveteesac-my.sharepoint.com/:w:/r/personal/e4132213_live_tees_ac_uk/Documents/year-2/web-apps-and-services/assessments/component-2/CIS2058-N%20Assessment%20Checklist.docx?d=w79503cfd2a76474abed490bccc44a2ad&csf=1&web=1&e=f2M5Px)

The internal system will be used by Apex staff to perform the key operations: 
  Manage event details; 
  Assign staff to events; 
  Attach guests to events; 
  Assign food orders to events. 

An existing web service will be used for reserving event venues.

the Catering will be created as an independent web service so it can be used as a third-party service for other companies.

MUST Functional Requirements 
Web Api Services (Apex.Catering) to: 
Create, edit, delete and list food items - see the ERD above for details; 
Create, edit, delete and list the details of food Menus - see the ERD above for details;  
Add and remove a food item from a menu - see the ERD above for details; 
Book, edit and cancel Food for an Event - see the ERD above for details.  The service should return the FoodBookingId as confirmation of the booking;  
Via the RAZOR PAGES web app, the user should be able to: 
Create a new Event, specifying as a minimum its title, date and EventType; 
Create, list and edit guests; 
Book (add) a Guest onto an Event; 
List Guests for an Event including a total count of guests; 
Register guest attendance for an Event; 
Display the details of an individual Guest, including information about the Events with which they are associated and their attendance; 
Edit an Event (except its date and type); 

SHOULD Functional Requirements 
Via the RAZOR PAGES web app, the user should be able to: 
Cancel (remove) the booking of a guest from an upcoming Event; 
Reserve an appropriate, available Venue for an Event via the Apex.Venues web service, freeing any previously associated Venue; 
Display a list of Events that includes summary information about the Guests and Venue within it; 
Create, list and edit Staff; 
Adjust the staffing of an Event, adding available staff or removing currently assigned staff; 
See appropriate warnings within the event list and staffing views when there is not a first aider assigned to an Event; 
Display the details of a Staff member, including information about upcoming Events at which they are assigned to work; 
Cancel (soft delete) an Event, freeing any associated Venue and Staff; 

WOULD Functional Requirements 
Via the RAZOR PAGES web app, the user should be able to: 
Display the details for an Event, which must include details of the Venue, Staff and Guests – this should be more detailed that the summary information found in the Event list; 
Permanently remove personal data by anonymising their Guest entity; 
Display a detailed list of available Venues, filtered by EventType and date range, and then create a new Event by picking a result; 
See appropriate warnings within the event list and staffing views when there is fewer than one member of staff per 10 guests assigned to an Event. 

User access control should restrict the following operations: 
Can create and edit staff details (Permitted Users: Managers); 
Adjust the staffing of an event (Permitted Users: Team Leaders or Managers); 
Permanently delete (Permitted Users: Team Leaders or Managers). 

Report:
Prepare a short (500-word) report to evaluate the completeness of the solution, own working practices, and highlight security features built or planned for the app/service. 

deliverables:
Assessment Checklist 
Report 
Test Plan 
Source: this directory should contain the entire source code for the system, including any necessary project and solution configuration files and any code documentation. Ensure that you have cleaned (i.e. deleted intermediate and non-essential files for) the project(s) by deleting the various vs, bin, and obj directories.  This directory is used for assessing your coding. 
Media: this directory should contain short (20-30 seconds) full-resolution movie captures (MP4 H.264) of the application in use. 

problems + solutions:
when programming the variables for the Catering class "FoodItems", the ERD diagram provided says to make the UnitPrice a float data type. However, I have made it a decimal data type because this data type is designed for use with currency and avoids potential subtle rounding errors that may occur with the float data type.

Issues occurred with the original copy so I have had to recreate the project. 
I've created the Data folder,
created the Menu, FoodItem, MenuFoodItem, FoodBooking, and CateringDbContext classes,
ive programmed them all (at least partially),
ive created the initial migration and updated it with the seed data programmed,
created a README document,
ran Build > Rebuild the solution to make sure their were no problems
