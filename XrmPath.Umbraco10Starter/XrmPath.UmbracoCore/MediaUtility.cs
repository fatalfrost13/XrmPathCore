﻿using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Definitions;
using XrmPath.UmbracoCore.BaseServices;

namespace XrmPath.UmbracoCore.Utilities
{
    /// <summary>
    /// Dependencies: Logger(optional), UmbracoHelper, MediaService
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class MediaUtility: BaseInitializer
    {
        public MediaUtility(ServiceUtility? serviceUtil): base(serviceUtil) { }

        public MediaItem? GetMediaItem(int id)
        {
            try
            {
                //first check media item via UmbracoHelper
                //this pulls from lucene index
                var typeMediaItem = umbracoHelper?.Media(id);
                if (typeMediaItem != null && (pcUtil?.NodeExists(typeMediaItem) ?? false))
                {
                    var mediaItem = new MediaItem
                    {
                        Id = typeMediaItem?.Id ?? 0,
                        Url = typeMediaItem?.Url() ?? "",
                        Name = typeMediaItem?.Name ?? ""
                    };
                    return mediaItem;
                    //return publishedMediaItem;
                }
                else if(mediaService != null)
                {
                    //lastly if we can't pull it from cache, we'll use the database service umbraco.library.
                    //according to https://shazwazza.com/post/ultra-fast-media-performance-in-umbraco/ results are cached so it only makes db call the first time.
                    var media = mediaService.GetById(id);
                    if (media != null)
                    {
                        var mediaItem = new MediaItem
                        {
                            Id = id,
                            Url = media.GetValue("umbracoFile")?.ToString() ?? "", //  SiteUrlUtility.GetSiteUrl(liveMediaItem.Values["umbracoFile"]),
                            Name = media.Name ?? ""
                        };
                        return mediaItem;
                        //return liveMediaItem;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaItem(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaItem(). URL Info: {UrlUtility.GetCurrentUrl()}");
            }
            return null;
        }
        public MediaItem? GetMediaItem(IPublishedContent? content, string alias)
        {
            if (content == null) {
                return null;
            }
            var mediaList = GetMediaList(content, alias);
            var media = mediaList.FirstOrDefault();
            return media;
        }
        public MediaItem? GetMediaItem(Udi? udi)
        {
            if (udi == null || umbracoHelper == null) {
                return null;
            }
            var id = umbracoHelper.Media(udi)?.Id ?? 0;
            if (id > 0)
            {
                var mediaItem = GetMediaItem(id);
                if (mediaItem != null)
                {
                    return mediaItem;
                }
            }
            return null;
        }
        public MediaItem? GetMediaItem(string? udiString)
        {
            if (string.IsNullOrEmpty(udiString)) {
                return null;
            }
            var parsedUdi = Udi.Create(udiString);
            if (parsedUdi != null)
            {
                var media = GetMediaItem(parsedUdi);
                if (media != null)
                {
                    return media;
                }
            }
            return null;
        }
        public string GetMediaPath(int nodeid)
        {
            var path = string.Empty;
            if (nodeid == 0) {
                return path;
            }
            try
            {
                var mediaItem = GetMediaItem(nodeid);
                var mediaItemUrl = mediaItem?.Url;
                if (mediaItemUrl != null)
                {
                    path = mediaItemUrl;
                    return path;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaItem(IContent, alias). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaItem(IContent, alias). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return path;
        }
        /// <summary>
        /// Pulls Media property from either library.GetMedia method or MediaService.GetById
        /// </summary>
        /// <param name="nodeid"></param>
        /// <param name="alias"></param>
        /// <param name="cachedVersion"></param>
        /// <returns></returns>
        public string? GetMediaProperty(int nodeid, string alias = "umbracoFile", bool cachedVersion = true)
        {
            var path = string.Empty;
            if (mediaService == null) {
                return path;
            }
            try
            {

                //get from database
                var m = mediaService.GetById(nodeid);
                if (m != null && m.Id > 0 && m.HasProperty(alias))
                {
                    path = m.GetValue(alias) != null ? m.GetValue(alias)?.ToString() : string.Empty;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.Web caught error on MediaUtility.GetMediaProperty(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaProperty(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Content node</param>
        /// <param name="alias">Field alias of media crop picker</param>
        /// <param name="cropAlias">Crop alias</param>
        /// <param name="renderAsExtension">Extension (Ex. png) if different from uploaded</param>
        /// <returns></returns>
        public string? GetMediaCropUrl(IPublishedContent? node, string alias, string cropAlias, string renderAsExtension = "")
        {
            var cropUrl = string.Empty;
            if (node == null || pcUtil == null || umbracoHelper == null) {
                return cropUrl;
            }
            try
            {
                var jsonValue = pcUtil.GetContentValue(node, alias);
                if (!string.IsNullOrEmpty(jsonValue))
                {
                    var publishedContent = umbracoHelper.Content(node.Id);
                    cropUrl = publishedContent != null ? publishedContent.GetCropUrl(alias, cropAlias) : string.Empty;

                    if (!string.IsNullOrEmpty(renderAsExtension))
                    {
                        var extension = StringUtility.GetExtensionFromRelativeUrlPath(cropUrl);
                        if (cropUrl?.IndexOf("format=", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(extension) && renderAsExtension != extension)
                        {
                            cropUrl = $"{cropUrl}&format={extension}";
                        }
                    }

                    var brightness = pcUtil.GetNodeDecimal(node, UmbracoCustomFields.CropperBrightness);
                    var contrast = pcUtil.GetNodeDecimal(node, UmbracoCustomFields.CropperContrast);
                    var saturation = pcUtil.GetNodeDecimal(node, UmbracoCustomFields.CropperSaturation);

                    if (brightness != 0)
                    {
                        cropUrl = $"{cropUrl}&brightness={brightness}";
                    }
                    if (contrast != 0)
                    {
                        cropUrl = $"{cropUrl}&contrast={contrast}";
                    }
                    if (saturation != 0)
                    {
                        cropUrl = $"{cropUrl}&saturation={saturation}";
                    }
                }
                //cropUrl = SiteUrlHelper.GetSiteUrl(cropUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaCropUrl(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaCropUrl(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return cropUrl;
        }
        public List<MediaItem> GetMediaList(IPublishedContent? content, string alias)
        {
            var mediaList = new List<MediaItem>();
            if (content == null) {
                return mediaList;
            }
            try
            {
                var files = pcUtil?.GetContentValue(content, alias);
                //mediaList = GetMediaList(files);
                if (files != null && files.Contains("umb://"))
                {
                    //Umbraco.MediaPicker2
                    var udisList = files.Split(',').ToList();
                    foreach (var udiValue in udisList)
                    {   
                        var parsedUdi = Udi.Create(udiValue);
                        if (parsedUdi != null)
                        {
                            var mediaItem = GetMediaItem(parsedUdi);
                            if (mediaItem != null)
                            {
                                mediaList.Add(mediaItem);
                            }
                        }
                    }
                }
                else
                {
                    //Umbraco.MediaPicker
                    var mediaIds = files?.Split(',') ?? Array.Empty<string>();

                    foreach (var mediaIdValue in mediaIds)
                    {
                        int mediaId;
                        bool validInt = int.TryParse(mediaIdValue, out mediaId);
                        if (validInt && mediaId > 0)
                        {
                            var filePath = GetMediaItem(mediaId)?.Url ?? string.Empty;
                            var relatedFile = new MediaItem
                            {
                                Id = mediaId,
                                Url = filePath
                            };
                            if (relatedFile != null)
                            {
                                mediaList.Add(relatedFile);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MediaUtility.GetMediaList(IPublishedContent). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return mediaList;
        }
    }
}
