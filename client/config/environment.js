/* eslint-env node */
'use strict';

module.exports = function (environment) {
  let ENV = {
    modulePrefix: 'client',
    podModulePrefix: 'client/pods',
    environment,
    rootURL: '/',
    locationType: 'auto',
    EmberENV: {
      FEATURES: {
        // Here you can enable experimental features on an ember canary build
        // e.g. 'with-controller': true
      },
      EXTEND_PROTOTYPES: {
        // Prevent Ember Data from overriding Date.parse.
        Date: false
      }
    },

    APP: {
      API_HOST: 'http://localhost:5000/api/status/gh',
      SOCKETS_HOST: 'http://localhost:5000/sockets/git-package-status'
    }
  };

  if (environment === 'development') {
    // ENV.APP.LOG_RESOLVER = true;
    // ENV.APP.LOG_ACTIVE_GENERATION = true;
    // ENV.APP.LOG_TRANSITIONS = true;
    // ENV.APP.LOG_TRANSITIONS_INTERNAL = true;
    // ENV.APP.LOG_VIEW_LOOKUPS = true;
  }

  if (environment === 'test') {
    // Testem prefers this...
    ENV.locationType = 'none';

    // keep test console output quieter
    ENV.APP.LOG_ACTIVE_GENERATION = false;
    ENV.APP.LOG_VIEW_LOOKUPS = false;

    ENV.APP.rootElement = '#ember-testing';
  }

  if (environment === 'production') {
    ENV.APP.API_HOST = 'http://dotnet-status.com/api/status/gh';
    ENV.APP.SOCKETS_HOST = 'http://dotnet-status.com/sockets/git-package-status';
  }

  return ENV;
};
