let allQuizzesTemplateUrl = require('ngtemplate!html!./all-quizzes/all-quizzes.html');
let popularQuizzesTemplateUrl = require('ngtemplate!html!./all-quizzes/popular-quizzes.html');
let quizTemplateUrl = require('ngtemplate!html!./quiz/quiz.html');
let addQuizTemplateUrl = require('ngtemplate!html!./add-quiz/add-quiz.html');
let statsTemplateUrl = require('ngtemplate!html!./stats/stats.html');
let resultsTemplateUrl = require('ngtemplate!html!./quiz-results/quiz-results.html');
let playQuizzesTemplateUrl = require('ngtemplate!html!./all-quizzes/play.html');
let timeOverTemplateUrl = require('ngtemplate!html!./quiz/time-over.html');
let errorTemplateUrl = require('ngtemplate!html!./errors/error.html');

/** @ngInject */
export default function ($stateProvider, $urlRouterProvider) {

    $urlRouterProvider.otherwise('/play/popular');

    $stateProvider
        .state('play', {
            abstract: true,
            url: '/play',
            templateUrl: playQuizzesTemplateUrl,
            controller: 'PlayQuizzesCtrl as playCtrl',
        })
            .state('play.popular', {
                url: '/popular',
                templateUrl: popularQuizzesTemplateUrl,
                controller: 'AllQuizzesCtrl as ctrl',
                params: {
                    count: 10
                }
            })
            .state('play.all', {
                url: '/all',
                templateUrl: allQuizzesTemplateUrl,
                controller: 'AllQuizzesCtrl as ctrl',
                params: {
                    count: 0
                }
            })
        .state('quiz', {
            url: '/quiz/:id',
            templateUrl: quizTemplateUrl,
            controller: 'QuizCtrl as ctrl'
        })
        .state('timeover', {
            url: '/timeover',
            templateUrl: timeOverTemplateUrl,
            controller: 'TimeOverCtrl as ctrl',
            params: {
                id: ""
            }
        })
        .state('addQuiz', {
            url: '/addQuiz',
            templateUrl: addQuizTemplateUrl,
            controller: 'AddQuizCtrl as ctrl'
        })
        .state('stats', {
            url: '/stats',
            templateUrl: statsTemplateUrl,
            controller: 'StatsCtrl as ctrl'
        })
        .state('quizResults', {
            url: '/results',
            templateUrl: resultsTemplateUrl,
            controller: 'QuizResultsCtrl as ctrl',
            params: {
                result: {}
            }
        })
        .state('error', {
            url: '/error',
            templateUrl: errorTemplateUrl,
            controller: 'ErrorCtrl as ctrl',
            params: {
                message: ""
            }
        });

}