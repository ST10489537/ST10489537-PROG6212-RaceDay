# RaceDay API Endpoint Plan

**Module:** PROG6212/w — Part 1
**Student:** Nkosana | ST10489537

This plan covers Authentication, User Profile, Events, Categories, Event Enrolments, and Results, matching the RaceDay ERD (see `ERD_RaceDay.png` in this folder). The API in Part 2 must implement these endpoints as specified — any deviation must be explained in the Part 2 README.

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user as either an Organiser or a Participant. | None (public) | { fullName, email, password, role } | 201 Created – user record (no password) · 409 Conflict – email already registered |
| POST | /api/auth/login | Authenticates a user and starts a session storing their UserID and role. | None (public) | { email, password } | 200 OK – session started, user + role returned · 401 Unauthorized – invalid credentials |
| POST | /api/auth/logout | Ends the current user's session. | Any (logged in) | None | 200 OK – session cleared |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/profile | Returns the logged-in user's own profile details. | Any (logged in) | None | 200 OK – profile object · 401 Unauthorized |
| PUT | /api/profile | Updates the logged-in user's own profile details. | Any (logged in) | { fullName, phoneNumber, profilePictureUrl } | 200 OK – updated profile · 400 Bad Request – validation failed |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all upcoming events. Supports optional filtering by event type. | Any (logged in) | None | 200 OK – array of events |
| GET | /api/events/{id} | Returns full detail for a single event, including its categories. | Any (logged in) | None | 200 OK – event object · 404 Not Found |
| POST | /api/events | Creates a new event owned by the logged-in Organiser. | Organiser | { name, description, eventDate, location, distanceKm, eventType, bannerImageUrl } | 201 Created – new event · 400 Bad Request |
| PUT | /api/events/{id} | Updates an event owned by the logged-in Organiser. | Organiser | { name, description, eventDate, location, distanceKm, eventType, bannerImageUrl } | 200 OK – updated event · 403 Forbidden – not the owner · 404 Not Found |
| DELETE | /api/events/{id} | Deletes an event owned by the logged-in Organiser. | Organiser | None | 204 No Content · 403 Forbidden · 404 Not Found |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Lists all categories available for an event. | Any (logged in) | None | 200 OK – array of categories |
| POST | /api/events/{eventId}/categories | Adds a new age/distance category to an event. | Organiser | { name, description } | 201 Created – new category · 403 Forbidden – not the event owner |
| PUT | /api/categories/{id} | Updates an existing category. | Organiser | { name, description } | 200 OK – updated category · 403 Forbidden · 404 Not Found |
| DELETE | /api/categories/{id} | Removes a category from an event. | Organiser | None | 204 No Content · 403 Forbidden · 404 Not Found |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{eventId}/enrolments | Enrols the logged-in Participant into an event under a chosen category. | Participant | { categoryId } | 201 Created – enrolment record · 409 Conflict – already enrolled |
| GET | /api/enrolments/my | Lists all events the logged-in Participant has enrolled in. | Participant | None | 200 OK – array of enrolments |
| GET | /api/events/{eventId}/enrolments | Lists all Participants enrolled in an event owned by the logged-in Organiser. | Organiser | None | 200 OK – array of enrolments · 403 Forbidden – not the event owner |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{enrolmentId}/results | Captures a finish time and position for a Participant's enrolment. | Organiser | { finishTime, finishPosition } | 201 Created – result record · 403 Forbidden – not the event owner · 404 Not Found |
| PUT | /api/results/{id} | Updates a previously captured result. | Organiser | { finishTime, finishPosition } | 200 OK – updated result · 403 Forbidden · 404 Not Found |
| GET | /api/results/my | Lists the logged-in Participant's personal results across all completed events. | Participant | None | 200 OK – array of results |
