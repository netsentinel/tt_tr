BEGIN TRANSACTION;

CREATE TABLE Surveys(
	Id SERIAL PRIMARY KEY,
	Title TEXT NOT NULL
);

CREATE TABLE Questions(
	Id SERIAL PRIMARY KEY,
	SurveyId INTEGER NOT NULL REFERENCES Surveys(Id) ON DELETE RESTRICT,
	Title TEXT NOT NULL
);

CREATE TABLE Answers(
	Id SERIAL PRIMARY KEY,
	QuestionId INTEGER NOT NULL REFERENCES Questions(Id) ON DELETE RESTRICT,
	Title TEXT NOT NULL
);

CREATE TABLE Interviews(
	Id SERIAL PRIMARY KEY,
	SurveyId INTEGER NOT NULL REFERENCES Surveys(Id) ON DELETE RESTRICT
);

CREATE TABLE InterviewAnswers(
	Id SERIAL PRIMARY KEY,
	InterviewId INTEGER NOT NULL REFERENCES Interviews(Id) ON DELETE RESTRICT,
	QuestionId INTEGER NOT NULL REFERENCES Questions(Id) ON DELETE RESTRICT,
	AnswerId INTEGER NOT NULL REFERENCES Answers(Id) ON DELETE RESTRICT,
	CONSTRAINT InterviewAnswer_unique_interview_question_answer UNIQUE 
		(InterviewId, QuestionId, AnswerId)
);

CREATE INDEX InterviewAnswer_interviewanswer_interview_question_answer_idx ON InterviewAnswers 
USING BTREE (InterviewId, QuestionId, AnswerId);

-- really needed?
	
CREATE TABLE Results(
	Id INT PRIMARY KEY,
	SurveyId INTEGER NOT NULL REFERENCES Surveys(Id) ON DELETE RESTRICT
);

CREATE TABLE ResultAnswers(
	Id INT PRIMARY KEY,
	InterviewId INTEGER NOT NULL REFERENCES Results(Id) ON DELETE RESTRICT,
	QuestionId INTEGER NOT NULL REFERENCES Questions(Id) ON DELETE RESTRICT,
	AnswerId INTEGER NOT NULL REFERENCES Answers(Id) ON DELETE RESTRICT,
	CONSTRAINT ResultAnswer_unique_interview_question_answer UNIQUE 
		(InterviewId, QuestionId, AnswerId)
);

CREATE INDEX ResultAnswer_interviewanswer_interview_question_answer_idx ON ResultAnswers 
USING BTREE (InterviewId, QuestionId, AnswerId);

COMMIT;