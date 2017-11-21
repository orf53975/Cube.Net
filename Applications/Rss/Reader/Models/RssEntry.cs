﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntryBase
    ///
    /// <summary>
    /// RSS フィードを購読する Web サイトの情報を保持する基底クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssEntryBase
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        /// 
        /// <summary>
        /// タイトルを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Title { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Parent
        /// 
        /// <summary>
        /// RSS エントリが属するカテゴリを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCategory Parent { get; set; }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssEntry
    ///
    /// <summary>
    /// RSS フィードを購読する Web サイトの情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssEntry : RssEntryBase
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        /// 
        /// <summary>
        /// RSS フィードを取得するための URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry.Json
        /// 
        /// <summary>
        /// JSON 解析用クラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public Uri Uri { get; set; }

            public Json(RssEntry src)
            {
                Title = src.Title;
                Uri   = src.Uri;
            }

            public RssEntry Convert(RssCategory src) => new RssEntry
            {
                Title  = Title,
                Uri    = Uri,
                Parent = src,
            };
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssCategory
    ///
    /// <summary>
    /// 登録サイトが属するカテゴリ情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssCategory : RssEntryBase
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// サブカテゴリ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<RssCategory> Categories { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Entries
        /// 
        /// <summary>
        /// 登録した Web サイト一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<RssEntry> Entries { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        /// 
        /// <summary>
        /// 表示項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssEntryBase> Items
            => Categories.Cast<RssEntryBase>().Concat(Entries);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry.Json
        /// 
        /// <summary>
        /// JSON 解析用クラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        internal class Json
        {
            [DataMember] string Title { get; set; }
            [DataMember] IEnumerable<Json> Categories { get; set; }
            [DataMember] IEnumerable<RssEntry.Json> Entries { get; set; }

            public Json(RssCategory src)
            {
                Title      = src.Title;
                Entries    = src.Entries?.Select(e => new RssEntry.Json(e));
                Categories = src.Categories?.Select(e => new Json(e));
            }

            public RssCategory Convert(RssCategory src)
            {
                var dest = new RssCategory
                {
                    Title  = Title,
                    Parent = src,
                };
                dest.Categories = Categories?.Select(e => e.Convert(dest)).ToList() ?? new List<RssCategory>();
                dest.Entries    = Entries.Select(e => e.Convert(dest)).ToList();
                return dest;
            }
        }

        #endregion
    }
}
