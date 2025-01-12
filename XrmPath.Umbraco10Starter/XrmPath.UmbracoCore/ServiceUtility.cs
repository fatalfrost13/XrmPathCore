﻿using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using XrmPath.UmbracoCore.BaseServices;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    public class ServiceUtility : BaseUtility
    {
        protected MultiUrlUtility? _urlUtil;
        protected MediaUtility? _mediaUtil;
        protected ContentUtility? _contentUtil;
        protected LoggingUtility? _loggingUtil;
        protected PublishedContentUtility? _pcUtil;
        protected SearchUtility? _searchUtil;
        protected QueryUtility? _queryUtil;
        protected MemberUtility? _memberUtil;
        protected MembershipUtility? _membershipUtil;

        public ServiceUtility(ILogger<object>? logger = null, UmbracoHelper? umbracoHelper = null, IMediaService? mediaService = null, IExamineManager? examineManager = null, IContentService? contentService = null, IContentTypeService? contentTypeService = null, IMemberManager? memberManager = null, IMemberSignInManager? memberSignInManager = null, IOptions<AppSettingsModel>? appSettings = null)
            : base(logger, umbracoHelper, mediaService, examineManager, contentService, contentTypeService, memberManager, memberSignInManager, appSettings)
        {
            if (_pcUtil == null) {
                _pcUtil = new PublishedContentUtility(this);
            }
        }

        /// <summary>
        /// This is an option to have a singleton instance of ServiceUtility to avoid creating this object multiple times.
        /// CreateInstance needs to be the first thing that runs. Then once that's created, we can pull the singleton 'Instance'.
        /// </summary>
        //private static Lazy<ServiceUtility>? lazy = null;
        //public static ServiceUtility CreateInstance(ILogger<object>? logger = null, UmbracoHelper? umbracoHelper = null, IMediaService? mediaService = null, IExamineManager? examineManager = null, IContentService? contentService = null, IContentTypeService? contentTypeService = null, IMemberManager? memberManager = null, IMemberSignInManager? memberSignInManager = null, IOptions<AppSettingsModel>? appSettings = null)
        //{
        //    if (lazy == null)
        //    {
        //        lazy = new Lazy<ServiceUtility>(() => new ServiceUtility(logger, umbracoHelper, mediaService, examineManager, contentService, contentTypeService, memberManager, memberSignInManager, appSettings));
        //    }
        //    return lazy.Value;
        //}
        //public static ServiceUtility? Instance
        //{
        //    get { return lazy?.Value; }
        //}

        public PublishedContentUtility? GetPublishedContentUtility()
        {
            return _pcUtil;
        }
        public UmbracoHelper? GetUmbracoHelper()
        {
            return _umbracoHelper;
        }
        public IMediaService? GetMediaService()
        {
            return _mediaService;
        }
        public IExamineManager? GetExamineManager()
        {
            return _examineManager;
        }
        public IContentService? GetContentService()
        {
            return _contentService;
        }
        public IContentTypeService? GetContentServiceType()
        {
            return _contentTypeService;
        }
        public IMemberManager? GetMemberManager()
        {
            return _memberManager;
        }
        public IMemberSignInManager? GetMemberSignInManager() 
        {
            return _memberSignInManager;
        }
        public ILogger<object>? GetLogger()
        {
            return _logger;
        }
        public AppSettingsModel? GetAppSettings()
        {
            return _appSettings;
        }
        public LoggingUtility? GetLoggingUtility()
        {
            if (_loggingUtil == null && _logger != null)
            {
                _loggingUtil = new LoggingUtility(this);
            }
            return _loggingUtil;
        }
        public MultiUrlUtility? GetMultiUrlUtility()
        {
            if (_urlUtil == null && _umbracoHelper != null)
            {
                _urlUtil = new MultiUrlUtility(this);
            }
            return _urlUtil;
        }
        public ContentUtility? GetContentUtility()
        {
            if (_contentUtil == null && _contentService != null && _umbracoHelper != null && _contentTypeService != null)
            {
                _contentUtil = new ContentUtility(this);
            }
            return _contentUtil;
        }
        public MediaUtility? GetMediaUtility()
        {
            if (_mediaUtil == null && _mediaService != null && _umbracoHelper != null)
            {
                _mediaUtil = new MediaUtility(this);
            }
            return _mediaUtil;
        }
        public SearchUtility? GetSearchUtility()
        {
            if (_searchUtil == null && _mediaService != null && _umbracoHelper != null && _examineManager != null)
            {
                _searchUtil = new SearchUtility(this);
            }
            return _searchUtil;
        }
        public QueryUtility? GetQueryUtility()
        {
            if (_queryUtil == null && _umbracoHelper != null)
            {
                _queryUtil = new QueryUtility(this);
            }
            return _queryUtil;
        }
        public MemberUtility? GetMemberUtility() 
        {
            if (_memberUtil == null)
            {
                _memberUtil = new MemberUtility(this);
            }
            return _memberUtil;
        }
        public MembershipUtility? GetMembershipUtility()
        {
            if (_membershipUtil == null && _memberSignInManager != null)
            {
                _membershipUtil = new MembershipUtility(this);
            }
            return _membershipUtil;
        }

    }
}
