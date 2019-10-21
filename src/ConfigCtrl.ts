export class OpcUaConfigControl {
  static templateUrl = 'partials/config.html';
  current: any;

  /** @ngInject */
  constructor($scope) {
    this.current.jsonData = this.current.jsonData || {};
    $scope.$watch('ctrl.current.url', url => {
      this.save();
    });
  }

  save() {
    this.current.jsonData.url = this.current.url;
  }
}
