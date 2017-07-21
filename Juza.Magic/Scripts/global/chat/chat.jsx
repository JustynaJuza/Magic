var Chat = React.createClass({

    getDefaultProps() {
        return {
            isAuthenticated: false,
            userName: '',
            chatRooms: []
        };
    },

    render() {

        var chatRooms = this.props.chatRooms
            .map((chatRoom, index) => <ChatRoom key={index} chatRoom={chatRoom } />)

        return (

            <div>
                <div id="chat-header-bar-from-react">

                    <input id="chat-add-user-btn" class="btn btn-default" type="button" value="+" />

                    <h3 id="chat-header">

                        Hello<span id="user-name">{this.props.userName}</span>!

                    </h3>

                </div>

                <div className="chat container">

                    <input id="chat-room-selection" class="btn btn-primary" type="hidden" value="default" />

                    <div id="chat-rooms-container">

                        { chatRooms }

                        <div id="user-profile-tooltip-container"></div>

                    </div>

                    <div class="chat-message-new-container container">

                        <input id="chat-message-new" class="form-control" type="text" />

                        <input id="chat-message-send-btn" class="btn btn-primary" type="button" value="Send" />

                    </div>

                </div>
            </div>
        );
    }
});

var ChatRoom = React.createClass({

    getDefaultProps() {
        return {
            id: '',
            name: '',
            allowClose: false,
            isPrivate: false,
            isGameRoom: false,
            users: [],
            messages: []
        };
    },

    render() {

        var users = this.props.users.map((user, index) => <ChatRoomUser key={index} user={user } />);
        var messages = this.props.messages.map((message, index) => <ChatRoomMessage key={index} message={message } />);

        return (

            <div>
                <div id="room-tab-{this.props.id}" className="chat-room-tab" style="">
                    <span className="chat-room-tab-name {this.props.allowClose ? '' : 'no-tab-close'}">

                        { this.props.name}

                    </span>

                    <span className="chat-room-tab-close-btn">x</span>
                </div>
                <div id="room-content-{this.props.id}" className="chat-room-content" style="">

                    <div className="chat-room-users-container container right">
                        <ul id="room-users-{this.props.id}" className="chat-room-users">

                            { users}

                        </ul>
                    </div>

                    <div id="room-messages-container-{this.props.id}" className="chat-room-messages-container container left">
                        <ul id="room-messages-{this.props.id}" className="chat-room-messages">

                            { messages}

                        </ul>
                    </div>

                </div>
            </div>
        );
    }
});

var ChatRoomUser = React.createClass({
    render() {
        return

        <li className="chat-user display-name" style="">
            {this.props.userName}
        </li>
    }
});

var ChatRoomMessage = React.createClass({
    render() {
        return

        <li class="chat-message clearfix">
            <span>{this.props.timeSent}</span>
            <span className="chat-message-sender display-name">{this.props.senderName}</span>
            <span>{this.props.message}</span>
        </li>
    }
});