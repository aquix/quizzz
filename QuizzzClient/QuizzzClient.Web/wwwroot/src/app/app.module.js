import routeConfig from './app.routes';
import App from './app.directive';
import ApiService from './api-services/api.service';
import StorageService from './utils/storage.service';
import RouteService from './utils/route.service';
import AddQuizCtrl from './add-quiz/add-quiz.controller';
import AllQuizzesCtrl from './all-quizzes/all-quizzes.controller';
import QuizCtrl from './quiz/quiz.controller';
import StatsCtrl from './stats/stats.controller';
import QuizResultsCtrl from './quiz-results/quiz-results.controller';
import SidenavCtrl from './sidenav/sidenav.controller';
import PlayQuizzesCtrl from './all-quizzes/play.controller';
import TimeOverCtrl from './quiz/time-over.controller';
import ErrorCtrl from './errors/error.controller';


angular.module('app', [ 'ui.router', 'ngFileUpload', 'LocalStorageModule', 'ngMaterial', 'cl.paging' ]);

angular.module('app')
    .config(routeConfig)
    .constant('apiPath', '/api/quiz')
    .value('userName', null)
    .directive('app', () => new App())
    .service('api', ApiService)
    .service('storageService', StorageService)
    .service('route', RouteService)
    .controller('AddQuizCtrl', AddQuizCtrl)
    .controller('AllQuizzesCtrl', AllQuizzesCtrl)
    .controller('QuizCtrl', QuizCtrl)
    .controller('StatsCtrl', StatsCtrl)
    .controller('QuizResultsCtrl', QuizResultsCtrl)
    .controller('SidenavCtrl', SidenavCtrl)
    .controller('PlayQuizzesCtrl', PlayQuizzesCtrl)
    .controller('TimeOverCtrl', TimeOverCtrl)
    .controller('ErrorCtrl', ErrorCtrl);

angular.module('app')
    .run(function ($window, storageService) {
        window.onbeforeunload = function () {
            storageService.save();
        };
    });