define(['loading', 'dialogHelper', 'globalize', 'emby-input', 'emby-button', 'emby-checkbox'], function (loading, dialogHelper, globalize) {
    'use strict';

    var pluginId = "8a8f5c7d-9e2a-4b6f-a3d1-5e8c9b7a6f4e";

    function insertPlaceholder(textareaId, placeholder) {
        var textarea = document.getElementById(textareaId);
        if (textarea) {
            var cursorPos = textarea.selectionStart;
            var textBefore = textarea.value.substring(0, cursorPos);
            var textAfter = textarea.value.substring(cursorPos);
            textarea.value = textBefore + placeholder + textAfter;
            textarea.selectionStart = textarea.selectionEnd = cursorPos + placeholder.length;
            textarea.focus();
        }
    }

    function showEmojiPicker(textareaId) {
        var emojis = [
            '😀', '😃', '😄', '😁', '😆', '😅', '🤣', '😂', '🙂', '🙃',
            '😉', '😊', '😇', '🥰', '😍', '🤩', '😘', '😗', '😚', '😙',
            '😋', '😛', '😜', '🤪', '😝', '🤑', '🤗', '🤭', '🤫', '🤔',
            '🤐', '🤨', '😐', '😑', '😶', '😏', '😒', '🙄', '😬', '🤥',
            '😌', '😔', '😪', '🤤', '😴', '😷', '🤒', '🤕', '🤢', '🤮',
            '🤧', '🥵', '🥶', '😎', '🤓', '🧐', '😕', '😟', '🙁', '☹️',
            '😮', '😯', '😲', '😳', '🥺', '😦', '😧', '😨', '😰', '😥',
            '😢', '😭', '😱', '😖', '😣', '😞', '😓', '😩', '😫', '🥱',
            '👍', '👎', '👌', '✌️', '🤞', '🤟', '🤘', '🤙', '👈', '👉',
            '👆', '👇', '☝️', '👋', '🤚', '🖐', '✋', '🖖', '👏', '🙌',
            '💪', '🦾', '🙏', '✍️', '💅', '🤳', '💃', '🕺', '👯', '🧘',
            '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '🤍', '🤎', '💔',
            '❣️', '💕', '💞', '💓', '💗', '💖', '💘', '💝', '💟', '⭐',
            '🌟', '✨', '💫', '🔥', '💥', '💯', '✅', '🎉', '🎊', '🎈',
            '🎁', '🏆', '🥇', '🥈', '🥉', '⚽', '🏀', '🏈', '⚾', '🎾',
            '🎬', '🎭', '🎨', '🎪', '🎮', '🎯', '🎲', '🎰', '🎳', '🎧',
            '🎤', '🎸', '🎹', '🎺', '🎻', '🥁', '📱', '💻', '🖥', '⌨️',
            '🖱', '🖨', '📷', '📹', '📺', '📻', '⏰', '⏱', '⏲', '🕐'
        ];

        var dialogHtml = '<div style="padding: 20px;">';
        dialogHtml += '<h2 style="margin-top: 0;">Select an Emoji</h2>';
        dialogHtml += '<div style="display: grid; grid-template-columns: repeat(10, 1fr); gap: 10px; max-width: 600px; max-height: 400px; overflow-y: auto;">';
        
        for (var i = 0; i < emojis.length; i++) {
            dialogHtml += '<button is="emby-button" type="button" class="raised emoji-btn" ';
            dialogHtml += 'data-emoji="' + emojis[i] + '" ';
            dialogHtml += 'style="padding: 10px; font-size: 1.5em; min-width: 40px;">';
            dialogHtml += emojis[i];
            dialogHtml += '</button>';
        }
        
        dialogHtml += '</div></div>';

        var dialogOptions = {
            removeOnClose: true,
            scrollY: false
        };

        var dlg = dialogHelper.createDialog(dialogOptions);
        dlg.classList.add('ui-body-a');
        dlg.classList.add('background-theme-a');
        dlg.style.maxWidth = '700px';

        dlg.innerHTML = dialogHtml;

        var emojiButtons = dlg.querySelectorAll('.emoji-btn');
        for (var j = 0; j < emojiButtons.length; j++) {
            emojiButtons[j].addEventListener('click', function() {
                var emoji = this.getAttribute('data-emoji');
                insertPlaceholder(textareaId, emoji);
                dialogHelper.close(dlg);
            });
        }

        dialogHelper.open(dlg);
    }

    function createEntryHtml(entry, index) {
        var html = '<div class="paper-card entry-card" style="padding: 1.5em; margin-top: 1em; border-left: 3px solid #52B54B;" data-entry-index="' + index + '">';
        html += '<div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1em;">';
        html += '<h3 style="margin: 0; color: #52B54B;">Entry ' + (index + 1) + '</h3>';
        html += '<button is="emby-button" type="button" class="btnRemoveEntry raised" data-entry-index="' + index + '" style="background-color: #cc3333;">';
        html += '<i class="md-icon">delete</i>';
        html += '<span>Remove</span>';
        html += '</button>';
        html += '</div>';
        
        html += '<div class="inputContainer">';
        html += '<label class="inputLabel inputLabelUnfocused" for="txtBannerText' + index + '">Banner Text</label>';
        html += '<input is="emby-input" type="text" id="txtBannerText' + index + '" value="' + (entry.Text || '') + '" />';
        html += '<div class="fieldDescription">Enter your banner text. Use placeholders to insert dynamic content.</div>';
        html += '</div>';

        html += '<div style="margin-top: 10px;">';
        html += '<button is="emby-button" type="button" class="btnEmojiPicker raised" ';
        html += 'data-textarea-id="txtBannerText' + index + '" ';
        html += 'style="padding: 5px 10px; font-size: 0.9em; background-color: #f39c12; margin-bottom: 10px;">';
        html += '<i class="md-icon">sentiment_satisfied</i>';
        html += '<span>Insert Emoji</span>';
        html += '</button>';
        html += '</div>';

        html += '<div style="margin-top: 10px;">';
        html += '<div class="fieldDescription" style="margin-bottom: 5px; font-weight: bold;">Insert Placeholders:</div>';
        html += '<div style="display: flex; flex-wrap: wrap; gap: 5px;">';
        
        var placeholders = [
            { name: 'Movie Count', value: '{MovieCount}' },
            { name: 'Show Count', value: '{ShowCount}' },
            { name: 'Episode Count', value: '{EpisodeCount}' },
            { name: 'Video Count', value: '{VideoCount}' },
            { name: 'Running Streams', value: '{RunningStreams}' },
            { name: 'Latest Movies', value: '{LatestMovies}' },
            { name: 'Latest Shows', value: '{LatestShows}' }
        ];

        for (var i = 0; i < placeholders.length; i++) {
            var ph = placeholders[i];
            html += '<button is="emby-button" type="button" class="btnInsertPlaceholder raised" ';
            html += 'data-textarea-id="txtBannerText' + index + '" data-placeholder="' + ph.value + '" ';
            html += 'style="padding: 5px 10px; font-size: 0.9em; background-color: #3498db;">';
            html += ph.name;
            html += '</button>';
        }
        
        html += '</div>';
        html += '</div>';

        html += '</div>';
        return html;
    }

    function renderEntries(page, entries) {
        var container = page.querySelector('#bannerEntriesContainer');
        
        var html = '';
        for (var i = 0; i < entries.length; i++) {
            html += createEntryHtml(entries[i], i);
        }
        container.innerHTML = html;

        var removeButtons = container.querySelectorAll('.btnRemoveEntry');
        for (var j = 0; j < removeButtons.length; j++) {
            removeButtons[j].addEventListener('click', function() {
                var index = parseInt(this.getAttribute('data-entry-index'));
                removeEntry(page, index);
            });
        }

        var emojiButtons = container.querySelectorAll('.btnEmojiPicker');
        for (var m = 0; m < emojiButtons.length; m++) {
            emojiButtons[m].addEventListener('click', function() {
                var textareaId = this.getAttribute('data-textarea-id');
                showEmojiPicker(textareaId);
            });
        }

        var placeholderButtons = container.querySelectorAll('.btnInsertPlaceholder');
        for (var k = 0; k < placeholderButtons.length; k++) {
            placeholderButtons[k].addEventListener('click', function() {
                var textareaId = this.getAttribute('data-textarea-id');
                var placeholder = this.getAttribute('data-placeholder');
                insertPlaceholder(textareaId, placeholder);
            });
        }
    }

    function addEntry(page) {
        var entries = collectEntries(page);
        entries.push({ Text: '' });
        renderEntries(page, entries);
    }

    function removeEntry(page, index) {
        var entries = collectEntries(page);
        if (entries.length <= 1) {
            require(['alert'], function (alert) {
                alert('You must have at least one entry.');
            });
            return;
        }
        entries.splice(index, 1);
        renderEntries(page, entries);
    }

    function collectEntries(page) {
        var entries = [];
        var container = page.querySelector('#bannerEntriesContainer');
        var entryCards = container.querySelectorAll('.entry-card');
        
        for (var i = 0; i < entryCards.length; i++) {
            var textarea = page.querySelector('#txtBannerText' + i);
            var entry = {
                Text: textarea ? textarea.value : ''
            };
            entries.push(entry);
        }
        
        return entries;
    }

    function loadConfig(page, config) {
        page.querySelector('#txtUpdateInterval').value = config.UpdateIntervalSeconds || 60;
        page.querySelector('#txtLatestMoviesCount').value = config.LatestMoviesCount || 5;
        page.querySelector('#txtLatestShowsCount').value = config.LatestShowsCount || 5;
        page.querySelector('#chkEnableLogging').checked = config.EnableLogging || false;
        
        var entries = config.BannerEntries;
        if (!entries || entries.length === 0) {
            entries = [{ Text: 'Welcome to Emby!' }];
        }
        
        renderEntries(page, entries);
        loading.hide();
    }

    function saveConfig(page) {
        loading.show();

        var entries = collectEntries(page);

        var config = {
            BannerEntries: entries,
            UpdateIntervalSeconds: parseInt(page.querySelector('#txtUpdateInterval').value) || 60,
            LatestMoviesCount: parseInt(page.querySelector('#txtLatestMoviesCount').value) || 5,
            LatestShowsCount: parseInt(page.querySelector('#txtLatestShowsCount').value) || 5,
            EnableLogging: page.querySelector('#chkEnableLogging').checked
        };

        ApiClient.updatePluginConfiguration(pluginId, config).then(function () {
            loading.hide();
            require(['toast'], function (toast) {
                toast('Settings saved successfully');
            });
            
            setTimeout(function() {
                window.location.reload();
            }, 1000);
        });
    }

    function updateBannerNow() {
        loading.show();

        ApiClient.getJSON(ApiClient.getUrl('Plugins/' + pluginId + '/UpdateBannerNow')).then(function () {
            loading.hide();
            require(['toast'], function (toast) {
                toast('Banner text updated');
            });
        }).catch(function() {
            loading.hide();
            require(['toast'], function (toast) {
                toast('Banner text updated');
            });
        });
    }

    function onSubmit(e) {
        e.preventDefault();
        saveConfig(this);
        return false;
    }

    return function (view) {
        view.addEventListener('viewshow', function () {
            loading.show();

            var page = this;
            var form = page.querySelector('#BannerTextConfigForm');
            form.addEventListener('submit', onSubmit.bind(page));

            var addButton = page.querySelector('#btnAddEntry');
            addButton.addEventListener('click', function() {
                addEntry(page);
            });

            var updateNowBtn = page.querySelector('#btnUpdateNow');
            updateNowBtn.addEventListener('click', updateBannerNow);

            ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                loadConfig(page, config);
            });
        });
    };
});
