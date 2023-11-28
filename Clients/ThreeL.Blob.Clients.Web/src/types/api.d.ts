//dtos
interface LoginRequestDto {
    username: string;
    password: string;
    origin: string;
}

interface LoginResponseDto {
    id: number,
    userName: string,
    role: string,
    accessToken: string,
}

interface QueryUsersResponseDto {
    count: number,
    users: Array<UserBriefResponseDto>
}

interface UserBriefResponseDto {
    id: number,
    userName: string,
    role: string,
    createdTime: Date,
    lastLoginTime: Date,
    isDeleted: boolean,
}

interface UserCreationDto {
    userName: string;
    password: string;
}

interface UserEditDto {
    id: number;
    userName: string;
    role: string;
    size: number?;
}

//redux common store
interface SimpleKeyValueObject {
    [key: string]: any
}
interface CommonStore {
    state: object;
    actions: SimpleKeyValueObject;
    actionNames: SimpleKeyValueObject;
}