﻿using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using QuizzzClient.DataAccess.Interfaces;
using QuizzzClient.Entities;
using QuizzzClient.Web.Identity.Entities;
using QuizzzClient.Web.Models.ApiViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuizzzClient.Web.Services
{
    public class QuizService
    {
        private readonly UserManager<User> userManager;
        private IUnitOfWork db;

        public QuizService(IUnitOfWork db, UserManager<User> userManager) {
            this.db = db;
            this.userManager = userManager;
        }

        public bool AddQuiz(string data) {
            QuizData quizData;

            try {
                if (IsJson(data)) {
                    quizData = JsonConvert.DeserializeObject<QuizData>(data);
                    AddQuizToDb(quizData);
                } else if (IsXml(data)) {
                    var doc = new XDocument(data);
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    quizData = JsonConvert.DeserializeObject<QuizData>(data);
                    AddQuizToDb(quizData);
                } else {
                    return false;
                }
            } catch (JsonException) {
                return false;
            }

            return true;
        }

        public AllPreviewsViewModel GetPreviews(int count, string category) {
            var quizzes = db.Quizzes.GetAll()
                .OrderByDescending(q => q.AttemptsCount)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category)) {
                var categoryId = db.Categories.Where(c => c.Name == category).FirstOrDefault()?.Id;

                if (categoryId != null) {
                    quizzes = quizzes.Where(q => q.CategoryId == categoryId);
                }
            }

            if (count != 0) {
                quizzes = quizzes.Take(count);
            }

            var quizPreviews = new List<QuizPreviewViewModel>(quizzes.Count());
            foreach (var quiz in quizzes) {
                quizPreviews.Add(new QuizPreviewViewModel {
                    Id = quiz.Id,
                    AttemptsCount = quiz.AttemptsCount,
                    PassesCount = quiz.PassesCount,
                    Name = quiz.Name,
                    Author = quiz.Author,
                    Category = db.Categories.Where(c => c.Id == quiz.CategoryId).FirstOrDefault()?.Name,
                    CountOfQuestions = quiz.Questions.Count()
                });
            }

            return new AllPreviewsViewModel {
                Quizzes = quizPreviews,
                Categories = db.Categories.GetAll().Select(c => c.Name).ToList()
            };
        }

        internal object AcceptQuiz(AcceptQuizViewModel data, object p) {
            throw new NotImplementedException();
        }

        public async Task<AcceptQuizResultViewModel> AcceptQuiz(AcceptQuizViewModel data, string userName) {
            Quiz quiz;
            try {
                quiz = db.Quizzes.Find(data.QuizId);
            } catch (Exception) {
                return null;
            }

            if (quiz.Questions.Count() != data.Answers.Count()) {
                return null;
            }

            var correctQuestionsCount = 0;
            for (int i = 0; i < quiz.Questions.Count(); i++) {
                var question = quiz.Questions.ElementAt(i);
                var isQuestionCorrect = true;

                for (int j = 0; j < question.Answers.Count(); j++) {
                    var answer = question.Answers.ElementAt(j);
                    if (answer.IsCorrect && !data.Answers.ElementAt(i).Contains(j) ||
                        !answer.IsCorrect && data.Answers.ElementAt(i).Contains(j)) {

                        isQuestionCorrect = false;
                    }
                }

                if (isQuestionCorrect) {
                    correctQuestionsCount++;
                }
            }

            // Save stats in db and return result
            quiz.AttemptsCount++;

            var isQuizPassed = IsTestPassed(correctQuestionsCount, quiz.Questions.Count());

            if (isQuizPassed) {
                quiz.PassesCount++;
            }

            db.Quizzes.Update(quiz);

            // Update user stats
            var user = await userManager.FindByNameAsync(userName);
            var currentQuizResult = user.BestResults?.Where(r => r.QuizId == quiz.Id).FirstOrDefault();

            if (currentQuizResult == null) {
                var newQuizResult = new QuizBestResult {
                    QuizId = quiz.Id,
                    Name = quiz.Name,
                    QuestionsCount = quiz.Questions.Count(),
                    PassedQuestionsCount = correctQuestionsCount,
                    IsPassed = isQuizPassed
                };

                if (user.BestResults == null) {
                    user.BestResults = new List<QuizBestResult>();
                }

                user.BestResults.Add(newQuizResult);
            } else {
                if (correctQuestionsCount >= currentQuizResult.PassedQuestionsCount) {
                    currentQuizResult.PassedQuestionsCount = correctQuestionsCount;
                    currentQuizResult.IsPassed = isQuizPassed;
                }
            }

            await userManager.UpdateAsync(user);

            return new AcceptQuizResultViewModel {
                Id = quiz.Id,
                AllQuestionsCount = quiz.Questions.Count(),
                PassedQuestionsCount = correctQuestionsCount,
                Success = isQuizPassed
            };
        }

        public QuizViewModel GetQuiz(string id) {
            var quiz = db.Quizzes.Find(id);
            if (quiz == null) {
                return null;
            }

            var quizViewModel = new QuizViewModel {
                Id = quiz.Id,
                Name = quiz.Name,
                Author = quiz.Author,
                Category = db.Categories.Where(c => c.Id == quiz.CategoryId).FirstOrDefault()?.Name,
                Questions = quiz.Questions.Select(q => new QuestionViewModel {
                    Id = q.Id,
                    QuestionBody = q.QuestionBody,
                    Answers = q.Answers.Select(a => a.AnswerBody)
                })
            };

            return quizViewModel;
        }

        public async Task<IEnumerable<QuizBestResult>> GetUserStats(string userName) {
            var user = await userManager.FindByNameAsync(userName);
            return user.BestResults;
        }

        #region Helpers

        private bool IsJson(string content) {
            if (content.First() == '{') {
                return true;
            }

            return false;
        }

        private bool IsXml(string content) {
            if (content.First() == '<') {
                return true;
            }

            return false;
        }

        private void AddQuizToDb(QuizData quizData) {
            Quiz quiz = new Quiz {
                Author = quizData.Author,
                Name = quizData.Name,
                Questions = quizData.Questions
            };

            // Check if there is quiz with the same name
            var namePattern = new Regex($@"{Regex.Escape(quiz.Name)}_\d*");
            var quizWithSameName = db.Quizzes.Where(q => namePattern.IsMatch(q.Name)).ToList();
            if (quizWithSameName.Count > 0) {
                quiz.Name = $"{quiz.Name}_{quizWithSameName.Count}";
            }
             
            var categoryName = quizData.Category;
            var existingCategory = db.Categories.Where(c => c.Name == categoryName).FirstOrDefault();

            var categoryId = "";

            if (existingCategory == null) {
                var newCategory = new Category {
                    Name = categoryName
                };
                db.Categories.Add(newCategory);
                categoryId = newCategory.Id;
            } else {
                categoryId = existingCategory.Id;
            }

            quiz.CategoryId = categoryId;

            db.Quizzes.Add(quiz);
        }

        private bool IsTestPassed(int passedQuestionsCount, int allQuestionsCount) {
            const double LIMIT = 0.5;

            if ((double)passedQuestionsCount / allQuestionsCount > LIMIT) {
                return true;
            }

            return false;
        }

        #endregion
    }
}
