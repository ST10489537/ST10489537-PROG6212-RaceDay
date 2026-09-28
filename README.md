# RaceDay — Part 1: System Planning and Database

**Module:** PROG6212/w — Programming 2B
**Student:** Nkosana Khumalo | ST10489537

## System Description
RaceDay is a full-stack event management platform for South African road running,
walking, and cycling events. It allows Event Organisers to create and manage events,
categories, and participant results, while Participants can browse events, enter them
under a chosen category, and track their personal results history.

This repository currently contains the Part 1 planning deliverables: the Entity
Relationship Diagram, the API endpoint plan, and the SQL database script.

## Roles
- **Organiser** — creates, edits, and deletes events; manages event categories;
  captures participant results; views all enrolments for their events.
- **Participant** — registers an account, browses events, enrols in an event under
  a chosen category, and views their own enrolments and results.

## Repository Structure
- `docs/ERD_RaceDay.png` — Entity Relationship Diagram
- `docs/API_Endpoint_Plan.md` — Full API endpoint plan
- `docs/RaceDay_Schema.sql` — SQL Server schema and seed data script
- `.github/workflows/docs-check.yml` — CI check confirming the above files exist

## CI
![Docs check passing](docs/CI_Success.png)

## Video Presentation
Watch the Part 1 walkthrough here: https://youtu.be/i1V5dF0jPGg

## AI Disclosure
AI assistance was used for planning support (endpoint table structure and workflow
setup guidance), reviewed and finalised by the me.