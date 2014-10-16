var keywordApp = angular.module('keywordApp', []);

keywordApp.controller('KeywordController', function ($scope, KeywordService) {
    $scope.Keywords = [
        { "Key": "나오/VV", "Value": 77 },
        { "Key": "엘카/NN", "Value": 71 },
        { "Key": "생각/NN", "Value": 56 },
        { "Key": "정도/NN", "Value": 55 },
        { "Key": "오르/NN", "Value": 55 },
        { "Key": "사람/NN", "Value": 54 },
        { "Key": "모르/VV", "Value": 51 },
        { "Key": "무기/NN", "Value": 50 }
    ];

    $scope.beginDate = "";
    $scope.endDate ="";

    $scope.getKeywords = function() {
        console.log("called");

        var beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        var endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        console.log(beginDate);
        console.log(endDate);

        KeywordService
            .requestKeywords(beginDate, endDate)
            .then(function (keywords) {
                $scope.Keywords = keywords;
            });
    };

    function getKeywords(beginDate, endDate) {
        console.log("called");
        KeywordService.requestKeywords(beginDate, endDate)
            .then(function (keywords) {
                $scope.Keywords = keywords;
                $scope.$apply();
            });
    };
});

keywordApp.service('KeywordService', function($http, $q) {

    return ({
        requestKeywords: requestKeywords,
    });

    function requestKeywords(beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/tera/keywords/" + beginDate + "/" + endDate),
        });

        return (request.then(handleSuccess, handleError));
    }

    function handleSuccess(response) {
        return (response.data);
    }

    function handleError(response) {
        if (!angular.isObject(response.data) || !response.data.message) {
            return ($q.reject("An unknown error occurred."));
        }

        // Otherwise, use expected error message.
        return ($q.reject(response.data.message));
    }
});