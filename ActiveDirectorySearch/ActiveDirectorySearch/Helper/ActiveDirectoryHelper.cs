using ActiveDirectorySearch.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace ActiveDirectorySearch.Helper
{
    public static class ActiveDirectoryHelper
    {
        public static AdUserModel GetUserDetailsFromLoginId(string samAccountName)
        {
            AdUserModel user = new AdUserModel()
            {
                AdLogin = samAccountName
            };

            try
            {
                if (!string.IsNullOrEmpty(samAccountName))
                {
                    using (var root = GetGlobalCatalog())
                    {
                        using (var searcher = new DirectorySearcher(root))
                        {
                            const string queryFormat = "(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))";

                            searcher.Filter = string.Format(queryFormat, samAccountName);
                            searcher.PropertiesToLoad.Add("*");

                            var result = searcher.FindOne();

                            if (result != null)
                            {
                                user.FirstName = GetInfoFromResult(result, "givenname");
                                user.LastName = GetInfoFromResult(result, "sn");
                                user.Department = GetInfoFromResult(result, "department");
                                user.Email = GetInfoFromResult(result, "mail");
                                user.DisplayName = GetInfoFromResult(result, "displayName");

                            }
                        }
                    }
                }
            }
            catch
            {
                //Ignore
            }

            return user;
        }

        public static string GetUserDisplayNameByEmail(string mailId)
        {
            string displayName = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(mailId))
                {
                    using (var root = GetGlobalCatalog())
                    {
                        using (var searcher = new DirectorySearcher(root))
                        {
                            const string queryFormat = "(&(objectCategory=person)(objectClass=user)(mail={0}))";

                            searcher.Filter = string.Format(queryFormat, mailId);
                            searcher.PropertiesToLoad.Add("displayName");

                            var result = searcher.FindOne();

                            if (result != null)
                            {
                                displayName = GetInfoFromResult(result, "displayName");
                            }
                            else
                            {
                                displayName = mailId;
                            }
                        }
                    }
                }
            }
            catch
            {
                //Ignore
            }
            return displayName;
        }

        #region Private Methods
        /// <summary>
        ///  Gets the global catalog.
        /// </summary>
        /// <returns></returns>
        private static DirectoryEntry GetGlobalCatalog()
        {
            var deGC = new DirectoryEntry("GC:");
            var ie = deGC.Children.GetEnumerator();
            ie.MoveNext();
            return (DirectoryEntry)ie.Current;
        }

        /// <summary>
        ///  Gets the info from result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetInfoFromResult(SearchResult result, string key)
        {
            try
            {
                return result.Properties[key][0].ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}